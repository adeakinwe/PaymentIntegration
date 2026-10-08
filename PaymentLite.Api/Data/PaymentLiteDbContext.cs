using Microsoft.EntityFrameworkCore;
using PaymentLite.Api.Models;

namespace PaymentLite.Api.Data
{
    public class PaymentLiteDbContext : DbContext
    {
        public PaymentLiteDbContext(DbContextOptions<PaymentLiteDbContext> options) : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Payment> Payments { get; set; }
    } 
}