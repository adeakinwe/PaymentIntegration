using PaymentLite.Api.Models;

namespace PaymentLite.Api.Interface
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByIdAsync(long id);

        Task AddAsync(Payment payment);

        Task<bool> SaveChangesAsync();
    }
}