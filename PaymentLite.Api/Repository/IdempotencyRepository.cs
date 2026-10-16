using Microsoft.EntityFrameworkCore;
using PaymentLite.Api.Data;
using PaymentLite.Api.Interface;

namespace PaymentLite.Api.Repository
{
    public class IdempotencyRepository : IIdempotencyRepository
    {
        private readonly PaymentLiteDbContext _context;

        public IdempotencyRepository(PaymentLiteDbContext context)
        {
            _context = context;
        }

        public async Task<IdempotencyRecord?> GetByKeyAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            return await _context.IdempotencyRecords
                .Include(x => x.Payment)
                .SingleOrDefaultAsync(
                    x => x.Key == key,
                    cancellationToken);
        }

        public async Task AddAsync(
            IdempotencyRecord record,
            CancellationToken cancellationToken = default)
        {
            await _context.IdempotencyRecords.AddAsync(
                record,
                cancellationToken);
        }
    }
}