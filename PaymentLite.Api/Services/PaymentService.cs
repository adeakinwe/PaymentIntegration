using PaymentLite.Api.Data;
using PaymentLite.Api.DTOs;
using PaymentLite.Api.Interface;
using PaymentLite.Api.Models;

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
            var sender = await _accountRepo.GetByIdAsync(paymentRequest.senderId);
            var receiver = await _accountRepo.GetByIdAsync(paymentRequest.receiverId);

            if (sender == null)
                throw new Exception("Sender Account not found");
            if (receiver == null)
                throw new Exception("Receiver Account not found");
            if (sender.Balance < paymentRequest.amount)
                throw new Exception("Insufficient balance");

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

            return payment;
        }
    }
}