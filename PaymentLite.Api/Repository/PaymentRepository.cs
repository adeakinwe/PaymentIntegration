using Microsoft.EntityFrameworkCore;
using PaymentLite.Api.Data;
using PaymentLite.Api.Interface;
using PaymentLite.Api.Models;

namespace PaymentLite.Api.Repository
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentLiteDbContext _context;
        public PaymentRepository(PaymentLiteDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
        }

        public async Task<Payment?> GetByIdAsync(long id)
        {
            var payment = await _context.Payments.Where(p => p.Id == id).FirstOrDefaultAsync();

            return payment;
        }

        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync() >= 0;
        }
    }
}