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

        public PaymentService(PaymentLiteDbContext context, IAccountRepository accountRepo, IPaymentRepository paymentRepo)
        {
            _context = context;
            _accountRepo = accountRepo;
            _paymentRepo = paymentRepo;
        }
        public async Task<Payment> ProcessPaymentAsync(PaymentRequest paymentRequest)
        {
            //await using var trans = await _context.Database.BeginTransactionAsync();

            await using (var tran = await _context.Database.BeginTransactionAsync())
            {
                try
                {
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


                    var payment = new Payment
                    {
                        SenderAccountId = paymentRequest.senderId,
                        ReceiverAccountId = paymentRequest.receiverId,
                        Amount = paymentRequest.amount,
                        Status = "completed",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _paymentRepo.AddAsync(payment);
                    await _paymentRepo.SaveChangesAsync();

                    await tran.CommitAsync();
                    await tran.DisposeAsync();
                    return payment;
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw new InvalidOperationException("Account modified by another transaction");
                }
                catch (DbUpdateException ex)
                {
                    await tran.RollbackAsync();
                    await tran.DisposeAsync();

                    throw new DbUpdateException(ex.Message);
                }
            }
        }
    }
}