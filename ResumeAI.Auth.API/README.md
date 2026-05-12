# ResumeAI — Auth Service

Full ASP.NET Core 8 Web API implementing the authentication and user management
microservice for the **ResumeAI** platform (.NET edition).

---

## Stack

| Concern           | Technology                                      |
|-------------------|-------------------------------------------------|
| Framework         | ASP.NET Core 8 Web API                          |
| ORM               | Entity Framework Core 8 (Npgsql provider)       |
| Database          | PostgreSQL 16                                   |
| Auth              | JWT Bearer · Google OAuth · PasswordHasher      |
| Docs              | Swashbuckle (OpenAPI 3.0)                       |
| Logging           | Serilog → Console + rolling file                |
| Containerisation  | Docker (multi-stage) + docker-compose           |

---

## Project Layout

```
ResumeAI.Auth.API/
├── Controllers/
│   └── AuthController.cs          # All auth REST endpoints
├── Data/
│   └── AuthDbContext.cs           # EF Core DbContext (PostgreSQL)
├── Entities/
│   ├── User.cs                    # users table
│   └── RefreshToken.cs            # refresh_tokens table
├── Enums/
│   └── Enums.cs                   # Role | AuthProvider | SubscriptionPlan
├── Extensions/
│   └── ServiceExtensions.cs       # DI registration helpers
├── Interfaces/
│   ├── IAuthService.cs
│   ├── IJwtService.cs
│   ├── IUserRepository.cs
│   └── IRefreshTokenRepository.cs
├── Middleware/
│   └── GlobalExceptionMiddleware.cs
├── Migrations/
│   ├── 20260422000001_InitialCreate.cs
│   └── AuthDbContextModelSnapshot.cs
├── Models/
│   ├── Requests/AuthRequests.cs
│   └── Responses/AuthResponses.cs
├── Repositories/
│   ├── UserRepository.cs
│   └── RefreshTokenRepository.cs
├── Services/
│   ├── AuthService.cs
│   └── JwtService.cs
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
├── Dockerfile
└── docker-compose.yml
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [PostgreSQL 16](https://www.postgresql.org/download/) **or** Docker Desktop

---

## Quick Start (Local)

### 1. Configure

Edit **`appsettings.Development.json`** (or set environment variables):

```json
{
  "ConnectionStrings": {
    "AuthDb": "Host=localhost;Port=5432;Database=resumeai_auth_dev;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "your-32+-character-secret-key-here!!",
    "Issuer": "ResumeAI.Auth",
    "Audience": "ResumeAI",
    "ExpiryHours": "24"
  },
  "Google": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com"
  }
}
```

### 2. Install packages & run

```bash
cd ResumeAI.Auth.API
dotnet restore
dotnet run
```

> EF Core migrations are applied automatically on startup (`MigrateAsync()`).

Swagger UI → **http://localhost:5000**

---

## Quick Start (Docker Compose)

```bash
docker compose up --build
```

Auth API → **http://localhost:5001**  
PostgreSQL → `localhost:5432` (user: `postgres`, pass: `postgres`)

---

## API Endpoints

| Method | Endpoint                     | Auth     | Description                          |
|--------|------------------------------|----------|--------------------------------------|
| POST   | `/api/auth/register`         | Public   | Register with email + password        |
| POST   | `/api/auth/login`            | Public   | Login, returns JWT + refresh token    |
| POST   | `/api/auth/logout`           | Bearer   | Revoke a refresh token                |
| POST   | `/api/auth/refresh`          | Public   | Rotate refresh token → new JWT        |
| POST   | `/api/auth/google`           | Public   | Login/register with Google ID token   |
| GET    | `/api/auth/profile`          | Bearer   | Get own profile                       |
| PUT    | `/api/auth/profile`          | Bearer   | Update name + phone                   |
| PUT    | `/api/auth/password`         | Bearer   | Change password (LOCAL only)          |
| PUT    | `/api/auth/subscription`     | Bearer   | Upgrade / downgrade plan              |
| DELETE | `/api/auth/deactivate`       | Bearer   | Soft-deactivate own account           |
| GET    | `/health`                    | Public   | Health check (PostgreSQL ping)        |

---

## EF Core Migrations (manual)

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> \
  --project ResumeAI.Auth.API.csproj \
  --output-dir Migrations

# Apply to the database
dotnet ef database update

# Rollback
dotnet ef database update <PreviousMigrationName>
```

---

## Authorization Policies

| Policy        | Claim Required                  |
|---------------|---------------------------------|
| `PremiumOnly` | `subscription == "PREMIUM"`     |
| `AdminOnly`   | `role == "ADMIN"`               |

Use on any controller or action:
```csharp
[Authorize(Policy = "PremiumOnly")]
[Authorize(Policy = "AdminOnly")]
```

---

## Environment Variables (Production)

```
ConnectionStrings__AuthDb=Host=...;Database=...;Username=...;Password=...
Jwt__Secret=<32+ char secret>
Jwt__Issuer=ResumeAI.Auth
Jwt__Audience=ResumeAI
Jwt__ExpiryHours=24
Google__ClientId=<client_id>
ASPNETCORE_ENVIRONMENT=Production
```

> **Never** commit real secrets. Use Azure Key Vault, AWS Secrets Manager, or
> Kubernetes Secrets in production.
