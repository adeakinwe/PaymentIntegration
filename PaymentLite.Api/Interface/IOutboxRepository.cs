
using PaymentLite.Api.Models;

namespace PaymentLite.Api.Interface
{
    public interface IOutboxRepository
    {
        Task AddAsync(OutboxMessage message);

        Task<List<OutboxMessage>> GetPendingMessagesAsync(
            int batchSize,
            CancellationToken cancellationToken = default);

        Task SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
