using Microsoft.EntityFrameworkCore;
using PaymentLite.Api.Data;
using PaymentLite.Api.Interface;
using PaymentLite.Api.Models;

namespace PaymentLite.Api.Repository
{
    public class OutboxRepository : IOutboxRepository
    {
        private PaymentLiteDbContext _context;

        public OutboxRepository(PaymentLiteDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(OutboxMessage message)
        {
            await _context.OutboxMessages.AddAsync(message);
        }

        public async Task<List<OutboxMessage>> GetPendingMessagesAsync(int batchSize, CancellationToken cancellationToken = default)
        {
            return await _context.OutboxMessages.Where(m => m.Status == "Pending")
            .OrderBy(m => m.CreatedAt).Take(batchSize).ToListAsync(cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}