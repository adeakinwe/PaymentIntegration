namespace PaymentLite.Api.Services
{
    using System.Net;
    using System.Net.Http.Json;
    using PaymentLite.Api.DTOs;
    using PaymentLite.Api.Interface;

    public class PaymentGateway : IPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly string baseUrl = "http://localhost:5147/api";
        public PaymentGateway(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PaymentGatewayResponse?> GetPaymentStatusAsync(
            string idempotencyKey,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(idempotencyKey))
                throw new ArgumentException(
                    "Idempotency key cannot be empty.",
                    nameof(idempotencyKey));

            using var response = await _httpClient.GetAsync(
                $"{baseUrl}/ExternalPayment/transaction-status/{idempotencyKey}",
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GatewayResponse>(
                cancellationToken: cancellationToken);

            if (result == null)
                throw new InvalidOperationException(
                    "Gateway returned an empty response.");

            return new PaymentGatewayResponse
            {
                Success = string.Equals(
                    result.Status,
                    "successful",
                    StringComparison.OrdinalIgnoreCase),

                Reference = result.Reference,
                Message = result.Message,
                Status = result.Status ?? "Unknown"
            };
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
                $"{baseUrl}/ExternalPayment/payment",
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
                Message = result?.Message,
                Status = result?.Status
            };
        }
    }
}