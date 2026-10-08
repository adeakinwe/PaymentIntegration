using Microsoft.EntityFrameworkCore;
using PaymentLite.Api.Data;
using PaymentLite.Api.Interface;
using PaymentLite.Api.Models;

namespace PaymentLite.Api.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly PaymentLiteDbContext _context;
        public AccountRepository(PaymentLiteDbContext context)
        {
            _context = context;
        }

        public async Task<Account?> GetByIdAsync(long id)
        {
            var account = await _context.Accounts.Where(a => a.Id == id).FirstOrDefaultAsync();

            return account;
        }

        public void Update(Account account)
        {
            _context.Accounts.Update(account);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() >= 0;
        }
    }
}