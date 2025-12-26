# Auth Service (LoanGateway.Auth)

## Overview
سرویس احراز هویت و مدیریت کاربران برای LoanGateway.
- مدیریت JWT و RefreshToken
- لاگین با OTP
- اتصال به سرویس‌های UID (Shahkar, PersonalInfo و …)
- ارسال پیامک از طریق Sms.Provider

## Projects
- `LoanGateway.Auth.Api` – لایه API (REST, Swagger, JWT)
- `LoanGateway.Auth.Application` – UseCaseها، Command/Query، Handlers
- `LoanGateway.Auth.Domain` – Aggregateها، Entities، ValueObjectها، Domain Events
- `LoanGateway.Auth.Infrastructure` – Repositoryها، DB Access، UID Providers، Sms Provider

## Local Run (without Docker)

```bash
cd Auth/LoanGateway.Auth.Api
dotnet run
