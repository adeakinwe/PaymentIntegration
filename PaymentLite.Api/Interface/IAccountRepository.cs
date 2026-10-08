using PaymentLite.Api.Models;

namespace PaymentLite.Api.Interface
{
    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(long id);

        void Update(Account account);
        Task<bool> SaveChangesAsync();
    }
}