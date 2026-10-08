namespace PaymentLite.Api.DTOs
{
    public class PaymentRequest
    {
        public long senderId { get; set; }
        public long receiverId { get; set; }
        public decimal amount { get; set; }
    }

    public class PaymentResponse
    {
        public long senderId { get; set; }
        public long receiverId { get; set; }
        public decimal amount { get; set; }
        public string status { get; set; } = null!;
        public DateTime dateCreated { get; set; }
    }
}