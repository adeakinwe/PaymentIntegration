using PaymentLite.Api.DTOs;

namespace PaymentLite.Api.Interface
{
public interface IPaymentGateway
{
    Task<PaymentGatewayResponse> ProcessPaymentAsync(
        decimal amount,
        string email,
        CancellationToken cancellationToken = default);
}
}