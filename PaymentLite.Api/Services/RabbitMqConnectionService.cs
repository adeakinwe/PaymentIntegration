using Microsoft.Extensions.Options;
using PaymentLite.Api;
using RabbitMQ.Client;

public class RabbitMqConnectionService : IHostedService, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqConnectionService> _logger;

    private IConnection? _connection;

    public IConnection Connection =>
        _connection ?? throw new InvalidOperationException(
            "RabbitMQ connection has not been established.");

    public RabbitMqConnectionService(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqConnectionService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,

            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),

            ClientProvidedName = "PaymentIntegration.API"
        };

        _connection = await factory.CreateConnectionAsync(
            cancellationToken);

        // Create a channel to declare the queue.
        await using var channel =
            await _connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _logger.LogInformation(
            "Connected to RabbitMQ. Queue: {QueueName}",
            _options.QueueName);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}