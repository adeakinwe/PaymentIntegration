public class IdempotencyRecord
{
    public Guid Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public Guid PaymentId { get; set; }

    public string Response { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Payment Payment { get; set; } = null!;
}