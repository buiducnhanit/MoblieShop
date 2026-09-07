# MoblieShop

MoblieShop is a mobile phone shopping application built with ASP.NET Core MVC, Entity Framework Core, SQL Server, ASP.NET Core Identity, and SignalR.

## Technology Stack

- .NET 8 and ASP.NET Core MVC
- Entity Framework Core with SQL Server
- ASP.NET Core Identity
- SignalR for real-time communication
- AutoMapper
- Docker and Docker Compose
- GitHub Actions and GitHub Container Registry (GHCR)

## Requirements

- .NET SDK 8
- Docker Desktop
- Git
- SQL Server when running outside Docker

## Project Structure

```text
MoblieShop/
├── Areas/          # Admin and Identity areas
├── Controllers/    # MVC controllers
├── Data/           # DbContext and seed data
├── Extensions/     # Application and middleware configuration
├── Hubs/           # SignalR hubs
├── Interface/      # Service and repository interfaces
├── Migrations/     # Entity Framework Core migrations
├── Models/         # Entity models
├── Repository/     # Data access implementations
├── Service/        # Business logic and external integrations
├── ViewModels/     # View models
└── Views/          # Razor views
```

## Run Directly with .NET

Run these commands from the solution directory:

```powershell
dotnet restore .\MoblieShop\MoblieShop.csproj
dotnet build .\MoblieShop\MoblieShop.csproj
dotnet run --project .\MoblieShop\MoblieShop.csproj
```

The application uses `http://localhost:5122` and `https://localhost:7209` with the HTTPS launch profile.

Set the environment explicitly when needed:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project .\MoblieShop\MoblieShop.csproj
```

Configuration is loaded in this order:

1. `appsettings.json`
2. `appsettings.{ASPNETCORE_ENVIRONMENT}.json`
3. Environment variables

Available environment configuration files:

- `Development`: `appsettings.Development.json`
- `Staging`: `appsettings.Staging.json`
- `Production`: `appsettings.Production.json`

## Run with Docker Compose

Do not put passwords, API keys, or other secrets directly in Compose files. Create a separate environment file for the environment you want to run.

### Development

```powershell
Copy-Item .\.env.development.example .\.env.development
docker compose --env-file .\.env.development `
  -f .\MoblieShop\docker-compose.development.yml up --build -d
```

Open `http://localhost:5122`.

### Staging

```powershell
Copy-Item .\.env.staging.example .\.env.staging
docker compose --env-file .\.env.staging `
  -f .\MoblieShop\docker-compose.staging.yml up --build -d
```

Open `http://localhost:5123`.

### Local Production

```powershell
Copy-Item .\.env.production.example .\.env.production
docker compose --env-file .\.env.production `
  -f .\MoblieShop\docker-compose.production.yml up --build -d
```

Open `http://localhost:5124`.

Stop an environment:

```powershell
docker compose --env-file .\.env.development `
  -f .\MoblieShop\docker-compose.development.yml down
```

Each environment uses its own SQL Server container and data volume. When running in a container, the application creates the database schema and seeds the default admin account.

## Secrets and Sensitive Configuration

The following files are safe templates and may be committed:

- `.env.development.example`
- `.env.staging.example`
- `.env.production.example`

Do not commit:

- `.env`
- `.env.development`
- `.env.staging`
- `.env.production`
- Certificates or private keys such as `*.pfx` and `*.pem`

Use .NET environment variable syntax for connection strings:

```text
ConnectionStrings__DefaultConnection=...
```

This overrides `ConnectionStrings:DefaultConnection` from the JSON configuration. Store credentials for Google, Facebook, PayPal, MoMo, VNPay, Cloudinary, email, and OpenAI in a secret manager or environment-specific secret store.

## Local Validation

Run these checks before pushing changes:

```powershell
dotnet test .\MoblieShop\MoblieShop.csproj --configuration Release --no-restore
docker build --check -f .\MoblieShop\Dockerfile .\MoblieShop
git diff --check
```

Validate a Compose file:

```powershell
docker compose --env-file .\.env.development `
  -f .\MoblieShop\docker-compose.development.yml config
```

## Default Admin Account

When a new database is created, the application seeds the following admin account:

```text
Email: admin@example.com
Password: Password123!
```

Change this password immediately when running outside a local development environment.
