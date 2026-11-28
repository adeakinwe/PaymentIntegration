using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using PaymentLite.Api;
using PaymentLite.Api.Data;
using PaymentLite.Api.Interface;
using PaymentLite.Api.Repository;
using PaymentLite.Api.Services;
using Polly;

var builder = WebApplication.CreateBuilder(args);

//Add Controller
builder.Services.AddControllers();

//Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Get password AND Register DbContext
string mysqlPassword = Environment.GetEnvironmentVariable("MYSQL_ROOT_PASSWORD") ?? throw new InvalidOperationException("Environment variable MYSQL_ROOT_PASSWORD is not set.");

string connectionString = builder.Configuration.GetConnectionString("Conn") ?? "";

//Inject password dynamically into connection string
if (!string.IsNullOrWhiteSpace(mysqlPassword) && connectionString.Contains("__DB_PASSWORD__"))
{
    connectionString = connectionString.Replace("__DB_PASSWORD__", mysqlPassword);
}

builder.Services.AddDbContext<PaymentLiteDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddHttpClient<IPaymentGateway, PaymentGateway>(
    client =>
    {
        client.BaseAddress = new Uri(
            "https://api.paymentgateway.co/");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                builder.Configuration["Gateway:SecretKey"]);

        // Request timeout control by Polly.
        client.Timeout = Timeout.InfiniteTimeSpan;
    }).AddResilienceHandler(
        "payment-gateway-circuit-breaker",
        static pipeline =>
        {
            //Circuit breaker
            pipeline.AddCircuitBreaker(
                new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    MinimumThroughput = 2,
                    SamplingDuration = TimeSpan.FromMinutes(1),
                    BreakDuration = TimeSpan.FromSeconds(30)
                });

            // Timeout
            pipeline.AddTimeout(
                TimeSpan.FromSeconds(10));
        });

//Register Repos and Services
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddHostedService<OutboxProcessor>();

//Register RabbitMQ Services
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMQ"));

builder.Services.AddSingleton<RabbitMqConnectionService>();

builder.Services.AddHostedService(provider =>
    provider.GetRequiredService<RabbitMqConnectionService>());

builder.Services.AddSingleton<RabbitMqPublisher>();

var app = builder.Build();
var env = app.Environment;

//app.UseHttpsRedirection();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

//app.UseAuthentication();
//app.UseAuthorization();
app.MapControllers();
app.Run();
