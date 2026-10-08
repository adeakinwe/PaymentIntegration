using Microsoft.EntityFrameworkCore;
using PaymentLite.Api.Data;
using PaymentLite.Api.Interface;
using PaymentLite.Api.Repository;
using PaymentLite.Api.Services;

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

//Register Repos and Services
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

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
