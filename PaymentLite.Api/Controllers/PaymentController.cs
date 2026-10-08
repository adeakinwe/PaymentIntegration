using Microsoft.AspNetCore.Mvc;
using PaymentLite.Api.DTOs;
using PaymentLite.Api.Services;

namespace PaymentLite.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController: ControllerBase
    {
        private readonly IPaymentService _paymentService;
        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                var payment = await _paymentService.ProcessPaymentAsync(request);

                if (payment != null)
                {
                    return Ok( new {success = true, result = payment, message = "payment succesful"});
                }
                return NotFound(new {success = false, message = "payment not successful"});
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}