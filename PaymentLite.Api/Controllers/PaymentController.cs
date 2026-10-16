using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentLite.Api.DTOs;
using PaymentLite.Api.Services;

namespace PaymentLite.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPaymentAsync(PaymentRequest request, [
            FromHeader(Name = "Idempotency-Key")] string? idempotencyKey, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(idempotencyKey))
                {
                    return BadRequest(new
                    {
                        message = "Idempotency-Key header is required."
                    });
                }
                var payment = await _paymentService.ProcessPaymentAsync(request, idempotencyKey, cancellationToken);

                if (payment != null)
                {
                    return Ok(new { success = true, result = payment, message = "payment succesful" });
                }
                return NotFound(new { success = false, message = "payment not successful" });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new
                {
                    message = "The account was modified by another transaction. Please retry."
                });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}