namespace PaymentLite.Api.Services
{
    using System.Net.Http.Json;
    using PaymentLite.Api.DTOs;
    using PaymentLite.Api.Interface;

    public class PaymentGateway : IPaymentGateway
    {
        private readonly HttpClient _httpClient;

        public PaymentGateway(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PaymentGatewayResponse> ProcessPaymentAsync(
            decimal amount,
            string email,
            string idempotencyKey,
            CancellationToken cancellationToken = default)
        {
            var request = new
            {
                amount = amount,
                email = email,
                idempotencyKey = idempotencyKey
            };

            var response = await _httpClient.PostAsJsonAsync(
                "http://localhost:5147/api/ExternalPayment/payment",
                request,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result =
                await response.Content.ReadFromJsonAsync<GatewayResponse>(
                    cancellationToken);

            return new PaymentGatewayResponse
            {
                Success = result?.Success ?? false,
                Reference = result?.Reference,
                Message = result?.Message
            };
        }
    }
}