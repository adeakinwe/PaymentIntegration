namespace PaymentLite.Api.Services
{
    using System.Net.Http.Headers;
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
            CancellationToken cancellationToken = default)
        {
            var request = new
            {
                amount = amount * 100, //kobo
                email = email,
                reference = Guid.NewGuid().ToString()
            };

            var response = await _httpClient.PostAsJsonAsync(
                "transaction/initialize",
                request,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result =
                await response.Content.ReadFromJsonAsync<GatewayResponse>(
                    cancellationToken);

            return new PaymentGatewayResponse
            {
                Success = result?.Status ?? false,
                Reference = result?.Data?.Reference,
                Message = result?.Message
            };
        }
    }
}