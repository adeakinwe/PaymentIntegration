using PaymentLite.Api.DTOs;
using PaymentLite.Api.Models;

namespace PaymentLite.Api.Services
{
    public interface IPaymentService
    {
        Task<Payment> ProcessPaymentAsync(PaymentRequest paymentRequest);
    }
}