
namespace PaymentLite.Api.DTOs
{
    public class PaymentInitRequestDTO
    {
        public long PaymentId { get; set; }

        public decimal Amount { get; set; }

        public string Email { get; set; } = string.Empty;

        public string IdempotencyKey { get; set; } = string.Empty;
    }
}
