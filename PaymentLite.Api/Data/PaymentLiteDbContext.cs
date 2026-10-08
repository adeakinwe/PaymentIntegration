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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.Property(a => a.AccountNumber)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(a => a.AccountNumber)
                    .IsUnique();

                 entity.Property(a => a.Balance)
                    .HasPrecision(18, 2)
                    .IsRequired();

                // entity.Property(a => a.AvailableBalance)
                //     .HasPrecision(18, 2)
                //     .IsRequired();

                // entity.Property(a => a.ReservedBalance)
                //     .HasPrecision(18, 2)
                //     .IsRequired();

                // entity.Property(a => a.RowVersion)
                //     .IsRowVersion();
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired();

                entity.Property(p => p.Status)
                    .IsRequired()
                    //.HasConversion<string>()
                    .HasMaxLength(20);

                entity.Property(p => p.CreatedAt)
                    .IsRequired();

                // entity.Property(p => p.IdempotencyKey)
                //     .IsRequired()
                //     .HasMaxLength(100);

                // entity.HasIndex(p => p.IdempotencyKey)
                //     .IsUnique();

                // entity.Property(p => p.ProviderReference)
                //     .HasMaxLength(100);

                // entity.Property(p => p.UpdatedAt)
                //     .IsRequired(false);

                // Sender relationship
                entity.HasOne<Account>()
                    .WithMany()
                    .HasForeignKey(p => p.SenderAccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Receiver relationship
                entity.HasOne<Account>()
                    .WithMany()
                    .HasForeignKey(p => p.ReceiverAccountId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Optimistic concurrency
                // entity.Property(p => p.RowVersion)
                //     .IsRowVersion();
            });
        }
    }
}