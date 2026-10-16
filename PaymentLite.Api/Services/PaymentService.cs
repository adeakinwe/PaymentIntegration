using Microsoft.EntityFrameworkCore;
using PaymentLite.Api.Data;
using PaymentLite.Api.DTOs;
using PaymentLite.Api.Interface;
using PaymentLite.Api.Models;
using PaymentLite.Api.Repository;

namespace PaymentLite.Api.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly PaymentLiteDbContext _context;
        private readonly IAccountRepository _accountRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly IPaymentGateway _paymentGateway;

        public PaymentService(PaymentLiteDbContext context, IAccountRepository accountRepo, IPaymentRepository paymentRepo, IPaymentGateway paymentGateway)
        {
            _context = context;
            _accountRepo = accountRepo;
            _paymentRepo = paymentRepo;
            _paymentGateway = paymentGateway;
        }
        public async Task<Payment> ProcessPaymentAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken = default)
        {
            var sender = await _accountRepo.GetByIdAsync(paymentRequest.senderId);
            var receiver = await _accountRepo.GetByIdAsync(paymentRequest.receiverId);

            if (paymentRequest.amount <= 0)
                throw new InvalidOperationException("Amount must be greater than zero");
            if (paymentRequest.email == null || !paymentRequest.email.Contains("@"))
                throw new InvalidOperationException("Invalid email address");
            if (sender == null)
                throw new InvalidOperationException("Sender Account not found");
            if (receiver == null)
                throw new InvalidOperationException("Receiver Account not found");
            if (sender.Balance < paymentRequest.amount)
                throw new InvalidOperationException("Insufficient balance");

            await Task.Delay(3000); //to test concurrency exception, simulate a delay in processing;
            sender.Balance -= paymentRequest.amount;
            receiver.Balance += paymentRequest.amount;

            // Initialize the payment OUTSIDE a database transaction.
            var gatewayResult =
                await _paymentGateway.ProcessPaymentAsync(
                    paymentRequest.amount,
                    paymentRequest.email,
                    cancellationToken);

            if (!gatewayResult.Success ||
                string.IsNullOrWhiteSpace(gatewayResult.Reference))
            {
                throw new InvalidOperationException(
                    gatewayResult.Message
                    ?? "Unable to initialize the payment.");
            }

            var payment = new Payment
            {
                SenderAccountId = paymentRequest.senderId,
                ReceiverAccountId = paymentRequest.receiverId,
                Amount = paymentRequest.amount,
                Status = "completed",
                CreatedAt = DateTime.UtcNow
            };

            await using (var tran = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _paymentRepo.AddAsync(payment);
                    await _paymentRepo.SaveChangesAsync(cancellationToken);

                    await tran.CommitAsync(cancellationToken);
                    await tran.DisposeAsync();
                    return payment;
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw new InvalidOperationException("Account modified by another transaction");
                }
                catch (DbUpdateException ex)
                {
                    await tran.RollbackAsync(cancellationToken);
                    await tran.DisposeAsync();

                    throw new DbUpdateException(ex.Message);
                }
            }
        }
    }
}