# JwtDemo

A minimal ASP.NET Core Web API (.NET 9) demonstrating JWT authentication and authorization with role-based access control and refresh token rotation.

## Features

- **JWT authentication** using `Microsoft.AspNetCore.Authentication.JwtBearer`
- **Role-based authorization** (`Admin`, `Worker`, `User`) via claims and policies
- **Refresh token rotation** with revocation support (in-memory store)
- **Minimal API** endpoints
- **OpenAPI** support in development

## Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

## Getting Started

```bash
dotnet restore
dotnet run
```

The API will be available at `http://localhost:5198` (HTTPS profile: `https://localhost:7232`).

## Demo Accounts

| Email                | Password   | Role   |
| -------------------- | ---------- | ------ |
| User@user.com        | Password   | User   |
| Admin@admin.com      | Password   | Admin  |
| Worker@worker.com    | Password   | Worker |

## Endpoints

| Method | Path            | Auth  | Role  | Description                          |
| ------ | --------------- | ----- | ----- | ------------------------------------ |
| GET    | `/`             | No    | -     | Health check                         |
| POST   | `/login`        | No    | -     | Returns access + refresh tokens      |
| POST   | `/refresh`      | No    | -     | Rotates a valid refresh token        |
| GET    | `/public`       | No    | -     | Public resource                      |
| GET    | `/public-secure`| Yes   | Any   | Any authenticated user               |
| GET    | `/admin`        | Yes   | Admin | Admin-only resource                  |
| GET    | `/worker`       | Yes   | Worker| Worker-only resource                 |

### Example: Login

```bash
curl -X POST http://localhost:5198/login \
  -H "Content-Type: application/json" \
  -d '{"email": "Admin@admin.com", "password": "Password"}'
```

Response:

```json
{
  "access_token": "<jwt>",
  "refresh_token": "<refresh-token>"
}
```

### Example: Access a protected endpoint

```bash
curl http://localhost:5198/admin \
  -H "Authorization: Bearer <access_token>"
```

### Example: Refresh tokens

```bash
curl -X POST http://localhost:5198/refresh \
  -H "Content-Type: application/json" \
  -d '{"refreshToken": "<refresh-token>"}'
```

## Configuration

JWT settings live in the `Authentication` section of `appsettings.json`:

```json
"Authentication": {
  "TokenSecret": "your-secret-key-at-least-32-chars",
  "RefreshTokenSecret": "your-secret-key-at-least-32-chars",
  "Issuer": "http://localhost:8080",
  "Audience": "my-web-api-client"
}
```

> **Note:** The token signing key is hard-coded in `Program.cs` and `AuthnticationExtensions.cs` for demo purposes. In production, always load secrets from secure configuration (e.g., environment variables, Azure Key Vault, or user secrets) and never commit real keys.

## Project Structure

| File | Description |
| ---- | ----------- |
| `Program.cs` | Minimal API setup, auth endpoints, JWT generation |
| `AppUser.cs` | Login request model and `Roles` enum |
| `AuthnticationExtensions.cs` | JWT bearer authentication configuration |
| `AuthAuthorizationExtensions.cs` | Role-based authorization policies |
| `AuthenticationSettings.cs` | Strongly-typed settings options |
| `RefreshTokenService.cs` | In-memory refresh token store (add/get/revoke/validate) |
| `RefreshToken.cs` | Refresh token model |
| `appsettings.json` | Configuration including `Authentication` section |

## Important Notes

- **Refresh tokens are stored in memory** (`ConcurrentDictionary`). They are lost on restart and do not survive multi-instance deployments. Use a persistent store (e.g., database or Redis) for production.
- The refresh endpoint **rotates** tokens: it revokes the old token and issues a new one on each refresh.
- Access tokens expire after **200 seconds** (`TokenLifetimeInSec` in `Program.cs`).

## License

This project is for demonstration purposes only. Use it as a reference, not as a production-ready authentication implementation.
