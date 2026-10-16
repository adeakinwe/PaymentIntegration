namespace PaymentLite.Api.DTOs
{
    public class PaymentRequest
    {
        public long senderId { get; set; }
        public long receiverId { get; set; }
        public decimal amount { get; set; }
        public string email { get; set; } = null!;
    }

    public class PaymentResponse
    {
        public long senderId { get; set; }
        public long receiverId { get; set; }
        public decimal amount { get; set; }
        public string status { get; set; } = null!;
        public DateTime dateCreated { get; set; }
    }

    public class PaymentGatewayResponse
    {
        public bool Success { get; set; }
        public string? Reference { get; set; }
        public string? Message { get; set; }
    }

    public class GatewayResponse
    {
        public bool Status { get; set; }
        public string? Message { get; set; }
        public GatewayData? Data { get; set; }
    }

    public class GatewayData
    {
        public string? Reference { get; set; }
        public string? AuthorizationUrl { get; set; }
    }
}