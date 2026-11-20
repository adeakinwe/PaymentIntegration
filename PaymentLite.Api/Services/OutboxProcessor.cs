
using System.Text.Json;
using PaymentLite.Api.DTOs;
using PaymentLite.Api.Interface;
using PaymentLite.Api.Models;

namespace PaymentLite.Api.Services
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxProcessor> _logger;
        private const int initialDelaySecs = 5;
        private const int maxDelaySecs = 60;
        private const int maxAttempts = 5;

        public OutboxProcessor(
            IServiceScopeFactory scopeFactory,
            ILogger<OutboxProcessor> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Outbox Processor started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingMessagesAsync(
                        stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error occurred while processing outbox messages.");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }

            _logger.LogInformation(
                "Outbox Processor stopped.");
        }

        private async Task ProcessPendingMessagesAsync(
            CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var outboxRepo = scope.ServiceProvider
                .GetRequiredService<IOutboxRepository>();

            var paymentRepo = scope.ServiceProvider
                .GetRequiredService<IPaymentRepository>();

            var paymentGateway = scope.ServiceProvider
                .GetRequiredService<IPaymentGateway>();

            var messages =
                await outboxRepo.GetPendingMessagesAsync(
                    10,
                    cancellationToken);

            foreach (var message in messages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Payment? payment = null;
                try
                {
                    // 1. Mark message as Processing.
                    message.Status = "Processing";

                    await outboxRepo.SaveChangesAsync(
                        cancellationToken);

                    // 2. Deserialize event payload.
                    var paymentEvent =
                        JsonSerializer.Deserialize<
                            PaymentInitRequestDTO>(
                                message.Payload)
                        ?? throw new InvalidOperationException(
                            "Invalid payment event payload.");

                    // 3. Retrieve the payment.
                    payment = await paymentRepo.GetByIdAsync(
                        paymentEvent.PaymentId);

                    if (payment == null)
                    {
                        throw new InvalidOperationException(
                            $"Payment {paymentEvent.PaymentId} not found.");
                    }

                    // 4. Initialize transaction
                    var gatewayResult =
                        await paymentGateway.ProcessPaymentAsync(
                            paymentEvent.Amount,
                            paymentEvent.Email,
                            paymentEvent.IdempotencyKey,
                            cancellationToken);

                    if (!gatewayResult.Success ||
                        string.IsNullOrWhiteSpace(
                            gatewayResult.Reference))
                    {
                        throw new InvalidOperationException(
                            gatewayResult.Message ??
                            "Payment initialization failed.");
                    }

                    // 5. Update payment.
                    payment.Status = "Initialized";
                    payment.GatewayReference =
                        gatewayResult.Reference;

                    // 6. Mark outbox message as processed.
                    message.Status = "Processed";
                    message.ProcessedAt = DateTime.UtcNow;
                    message.NextAttemptAt = null;
                    message.LastError = null;

                    await outboxRepo.SaveChangesAsync(
                        cancellationToken);

                    _logger.LogInformation(
                        "Payment {PaymentId} initialized successfully.",
                        payment.Id);
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    message.AttemptCount++;
                    message.LastError = ex.Message.Length > 2000
                        ? ex.Message[..2000]
                        : ex.Message;

                    var shouldRetry = IsTransientException(ex) && message.AttemptCount < maxAttempts;
                    if (payment != null)
                    {
                        payment.Status = "InitializationFailed";
                    }

                    if (shouldRetry)
                    {
                        var delay = CalculateRetryDelay(
                            message.AttemptCount);

                        message.Status = "Pending";
                        message.NextAttemptAt = DateTime.UtcNow.Add(delay);

                        _logger.LogWarning(
                            ex,
                            "Outbox message {MessageId} failed. " +
                            "Retry {AttemptCount}/{MaxAttempts} scheduled at {NextAttemptAt}.",
                            message.Id,
                            message.AttemptCount,
                            maxAttempts,
                            message.NextAttemptAt);
                    }
                    else
                    {
                        message.Status = "Failed";
                        message.NextAttemptAt = null;

                        if (payment != null)
                        {
                            payment.Status = "InitializationFailed";
                        }

                        _logger.LogError(
                            ex,
                            "Outbox message {MessageId} permanently failed " +
                            "after {AttemptCount} attempts.",
                            message.Id,
                            message.AttemptCount);
                    }

                    await outboxRepo.SaveChangesAsync(
                        cancellationToken);
                }
            }
        }

        private static TimeSpan CalculateRetryDelay(int attemptCount)
        {
            var delay = Math.Min(initialDelaySecs * Math.Pow(2, attemptCount - 1), maxDelaySecs);

            // Add random variation between 80% and 120%.
            var jitter = Random.Shared.NextDouble() * 0.4 + 0.8;

            return TimeSpan.FromSeconds(
                delay * jitter);
        }

        private static bool IsTransientException(Exception ex)
        {
            if (ex is HttpRequestException httpException)
            {
                if (httpException.StatusCode is not { } statusCode)
                    return true;

                return httpException.StatusCode == null ||
                       httpException.StatusCode ==
                           System.Net.HttpStatusCode.RequestTimeout ||
                       httpException.StatusCode ==
                           System.Net.HttpStatusCode.TooManyRequests ||
                       (int)httpException.StatusCode >= 500;
            }

            if (ex is TaskCanceledException)
            {
                return true;
            }

            return false;
        }
    }
}
