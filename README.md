# PaymentLite

PaymentLite is a .NET ASP.NET Core Web API that models a lightweight payment orchestration service. It validates account balances, enforces idempotent payment processing, persists payment state in MySQL, and delegates external payment initialization to a payment gateway.

## Overview

The service exposes payment endpoint that:

- validates a request payload and required idempotency header
- checks sender and receiver account existence
- verifies sufficient balance before processing
- calls an external gateway to initialize a payment transaction
- persists the payment and idempotency record atomically
- protects against duplicate processing via an idempotency key

## Technology Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8
- Pomelo.EntityFrameworkCore.MySql
- MySQL
- Swagger / OpenAPI
- Paystack HTTP API

## Solution Structure

```text
PaymentLite/
├── PaymentLite.Api/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Interface/
│   ├── Migrations/
│   ├── Models/
│   ├── Repository/
│   ├── Services/
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── PaymentLite.Api.csproj
│   ├── Program.cs
│   └── PaymentLite.Api.http
├── PaymentLite.sln
├── SETUP.md
└── README.md
```

## Core Components

### API Layer

`PaymentController` is the entry point for payment requests.

- Route: `POST /api/payment`
- Required header: `Idempotency-Key`
- Request body: `PaymentRequest`

### Service Layer

`PaymentService` implements the business workflow.

The service enforces the following logic:

1. Validate `Idempotency-Key` presence and length.
2. Check for existing idempotency record.
3. Validate request:
   - positive amount
   - valid email format
4. Load sender and receiver accounts.
5. Validate account existence and balance sufficiency.
6. Simulate an intentional processing delay to expose concurrency behaviour.
7. Update account balances.
8. Call external payment gateway to initialize the transaction.
9. Persist a `Payment` record and `IdempotencyRecord` within a database transaction.
10. Commit transaction and return the created payment.

### Data Access

`PaymentLiteDbContext` configures the schema for:

- `Accounts`
- `Payments`
- `IdempotencyRecords`

Important schema notes:

- `Account.AccountNumber` is unique.
- `Account.Balance` is stored with precision `18,2`.
- `Account.Version` is used as a concurrency token.
- `IdempotencyRecord.Key` is unique and required.
- `Payment` records are linked one-to-one with `IdempotencyRecord` via `PaymentId`.

### Repositories

The repository layer abstracts persistence concerns:

- `IAccountRepository`
- `IPaymentRepository`
- `IIdempotencyRepository`

The repositories wrap the EF Core context and keep controller/service code focused on business logic rather than persistence details.

### Gateway Integration

`PaymentGateway` is wired through `HttpClient` and configured to target Paystack:

- Base URL: `https://api.paystack.co/`
- Authorization header: `Bearer <secret>`
- Endpoint called: `transaction/initialize`

The gateway returns a normalized `PaymentGatewayResponse` containing:

- `Success`
- `Reference`
- `Message`

## Request Contract

### `POST /api/payment`

Headers:

```http
Idempotency-Key: <unique-client-generated-key>
Content-Type: application/json
```

Body:

```json
{
  "senderId": 1,
  "receiverId": 2,
  "amount": 250.50,
  "email": "customer@example.com"
}
```

Example successful response:

```json
{
  "success": true,
  "result": {
    "id": 12,
    "senderAccountId": 1,
    "receiverAccountId": 2,
    "amount": 250.50,
    "status": "completed",
    "createdAt": "2026-10-16T12:00:00Z"
  },
  "message": "payment succesful"
}
```

Example error response:

```json
{
  "message": "Idempotency-Key header is required."
}
```

## Idempotency Model

Idempotency is implemented using a dedicated `IdempotencyRecord` table and a required `Idempotency-Key` request header.

Behaviour:

- same key + same request should not re-process the payment
- duplicate requests are rejected with an exception path in the service layer
- the stored response payload is intended to allow replay-safe retrieval of gateway response details

This pattern is essential for avoiding duplicate charges and ensuring safe retry semantics in distributed or client-retry scenarios.

## Concurrency and Data Integrity

The application includes optimistic concurrency handling using `Account.Version` and the database transaction around payment creation.

The transaction scope is designed to:

- update account balances
- create the payment record
- create the idempotency record
- commit or rollback as a single unit

An intentional `Task.Delay(3000)` is used to simulate processing latency and surface concurrency edge cases during testing.

## Configuration

The project reads runtime configuration from `appsettings.json` and environment variables.

Example configuration:

```json
{
  "ConnectionStrings": {
    "Conn": "Server=**;Port=**;Database=**;User=**;Password=__DB_PASSWORD__;"
  },
  "Gateway": {
    "SecretKey": "your-secret-key"
  }
}
```

### Required Environment Variable

```bash
export MYSQL_ROOT_PASSWORD="your_mysql_password"
```

`Program.cs` injects this value into the connection string by replacing the `__DB_PASSWORD__` placeholder at startup.

## Local Setup

### Prerequisites

- .NET 8 SDK
- MySQL Server
- Access to a Paystack secret key

### Install dependencies

```bash
dotnet restore
```

### Create database and run migrations

```bash
dotnet ef database update
```

If migrations are not yet present, generate them first:

```bash
dotnet ef migrations add initmigration
```

### Run the API

```bash
dotnet run --project PaymentLite.Api
```

Then open:

- Swagger UI: `http://localhost:5217/swagger` (URL may vary depending on launch settings)

## Build and Publish

```bash
dotnet build

dotnet publish -c Release
```

## Notes and Production Considerations

This project is intentionally minimal but demonstrates several production-relevant concerns:

- external gateway integration via `HttpClient`
- transaction boundaries for payment persistence
- idempotency enforcement for safe retries
- optimistic concurrency checks on account updates
- persistence of gateway metadata for audit/reconciliation

## Reference Files

- `PaymentLite.Api/Program.cs` — startup configuration, dependency injection, swagger, database wiring
- `PaymentLite.Api/Controllers/PaymentController.cs` — API endpoint contract
- `PaymentLite.Api/Services/PaymentService.cs` — transactional payment orchestration logic
- `PaymentLite.Api/Data/PaymentLiteDbContext.cs` — EF Core schema configuration
- `PaymentLite.Api/Services/PaymentGateway.cs` — external gateway integration
- `SETUP.md` — supplementary setup instructions
