using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace PaymentLite.Api.Services
{
    public class RabbitMqPublisher
    {
        private readonly RabbitMqConnectionService _connectionService;
        private readonly RabbitMqOptions _options;

        public RabbitMqPublisher(
            RabbitMqConnectionService connectionService,
            IOptions<RabbitMqOptions> options)
        {
            _connectionService = connectionService;
            _options = options.Value;
        }

        public async Task PublishTestMessageAsync(CancellationToken cancellationToken = default)
        {
            var message = new
            {
                Message = "Hello from PaymentIntegration",
                CreatedAt = DateTime.UtcNow
            };

            var body = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(message));

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                Persistent = true,
                MessageId = Guid.NewGuid().ToString()
            };

            var channelOptions = new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true);

            await using var channel =
                await _connectionService.Connection.CreateChannelAsync(
                    channelOptions,
                    cancellationToken);

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _options.QueueName,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }
    }
}