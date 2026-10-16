namespace PaymentLite.Api.Interface
{
    public interface IIdempotencyRepository
    {
        Task<IdempotencyRecord?> GetByKeyAsync(
            string key,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            IdempotencyRecord record,
            CancellationToken cancellationToken = default);
    }
}