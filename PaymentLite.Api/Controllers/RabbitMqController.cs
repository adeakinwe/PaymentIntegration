using Microsoft.AspNetCore.Mvc;
using PaymentLite.Api.Services;

namespace PaymentLite.Api.Controllers
{
    [ApiController]
    [Route("api/rabbitmq")]
    public class RabbitMqController : ControllerBase
    {
        private readonly RabbitMqPublisher _publisher;

        public RabbitMqController(RabbitMqPublisher publisher)
        {
            _publisher = publisher;
        }

        [HttpPost("test-publish")]
        public async Task<IActionResult> PublishTestMessage(
            CancellationToken cancellationToken)
        {
            await _publisher.PublishTestMessageAsync(cancellationToken);

            return Ok(new
            {
                Message = "Test message published successfully."
            });
        }
    }
}