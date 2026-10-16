using System.Text.Json;
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
        private readonly IIdempotencyRepository _idempotencyRepo;

        public PaymentService(PaymentLiteDbContext context, IAccountRepository accountRepo,
        IPaymentRepository paymentRepo, IIdempotencyRepository idempotencyRepo, IPaymentGateway paymentGateway)
        {
            _context = context;
            _accountRepo = accountRepo;
            _paymentRepo = paymentRepo;
            _paymentGateway = paymentGateway;
            _idempotencyRepo = idempotencyRepo;
        }
        public async Task<Payment> ProcessPaymentAsync(PaymentRequest paymentRequest, string idempotencyKey, CancellationToken cancellationToken = default)
        {
            // Validate the idempotency key.
            if (string.IsNullOrWhiteSpace(idempotencyKey) ||
                idempotencyKey.Length > 100)
            {
                throw new ArgumentException(
                    "A valid idempotency key is required.",
                    nameof(idempotencyKey));
            }

            idempotencyKey = idempotencyKey.Trim();

            // check if Idempotency record exist.
            var existingIdempotencyRecord =
                await _idempotencyRepo.GetByKeyAsync(
                    idempotencyKey,
                    cancellationToken);

            if (existingIdempotencyRecord is not null)
            {
                throw new InvalidOperationException(
                         "Payment record already exist.");
                // return JsonSerializer.Deserialize<PaymentResponse>(
                //     existingIdempotencyRecord.Response)
                //     ?? throw new InvalidOperationException(
                //         "The stored payment response is invalid.");
            }
            
            if (paymentRequest.amount <= 0)
                throw new InvalidOperationException("Amount must be greater than zero");
            if (string.IsNullOrEmpty(paymentRequest.email) || !paymentRequest.email.Contains("@"))
                throw new InvalidOperationException("Invalid email address");

            var sender = await _accountRepo.GetByIdAsync(paymentRequest.senderId);
            var receiver = await _accountRepo.GetByIdAsync(paymentRequest.receiverId);

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
                    idempotencyKey,
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

            var idempotencyRecord = new IdempotencyRecord
            {
                Key = idempotencyKey,
                PaymentId = payment.Id,
                Response = JsonSerializer.Serialize(gatewayResult),
                CreatedAt = DateTime.UtcNow,
                Payment = payment
            };
            await using (var tran = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _paymentRepo.AddAsync(payment);
                    await _idempotencyRepo.AddAsync(idempotencyRecord, cancellationToken);
                    
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