using Microsoft.EntityFrameworkCore;
using PaymentLite.Api.Data;
using PaymentLite.Api.Interface;
using PaymentLite.Api.Repository;
using PaymentLite.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Register DbContext
string connectionString = builder.Configuration.GetConnectionString("Conn") ?? "";
builder.Services.AddDbContext<PaymentLiteDbContext>(options =>
        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

//Register Repos and Services
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
