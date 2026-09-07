# WebsiteBanHang
Đồ án cơ sở

## Run with Docker

1. Copy the environment example file and set a private strong SQL Server password.
2. Start the environment you need:

```powershell
Copy-Item .\.env.development.example .\.env.development
docker compose --env-file .\.env.development -f .\docker-compose.development.yml up --build -d
```

Development runs at http://localhost:5122. Use the equivalent commands for staging or production:

```powershell
Copy-Item .\.env.staging.example .\.env.staging
docker compose --env-file .\.env.staging -f .\docker-compose.staging.yml up --build -d

Copy-Item .\.env.production.example .\.env.production
docker compose --env-file .\.env.production -f .\docker-compose.production.yml up --build -d
```

The staging and production examples use ports `5123` and `5124` respectively.

Never commit `.env` or real API keys. Configure production secrets through the deployment platform's environment variables or secret manager.

## Environment configuration

The application loads the shared `appsettings.json` and then overrides it with the file matching `ASPNETCORE_ENVIRONMENT`:

- `Development`: `appsettings.Development.json`
- `Staging`: `appsettings.Staging.json`
- `Production`: `appsettings.Production.json`

Set the environment when running locally:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Staging"
dotnet run --project .\MoblieShop\MoblieShop.csproj
```

Keep connection strings and API credentials in environment variables or a secret manager. For example, `ConnectionStrings__DefaultConnection` overrides `ConnectionStrings:DefaultConnection`.

## GitHub Actions CI/CD

The workflow at `.github/workflows/master_mobile-shop.yml` follows a build-once/promote model:

```text
feature/* -> PR -> dev -> Docker image sha-<commit>
										|
								 tag v1.2.0
										|
									stag
										|
					  manual approval -> production
```

- Pull requests to `dev`, `stag`, or `master` run restore, build, test, and publish checks.
- A push to `dev` builds the Docker image once and publishes the immutable `sha-<full-commit>` tag plus the `dev` alias to GHCR.
- A version tag such as `v1.2.0` runs CI only; it never promotes automatically.
- Run the workflow manually with `promote_to=staging` and `source_tag=sha-<dev-commit>` to request staging promotion. GitHub Environment `staging` pauses the job for manual approval.
- After staging validation, run the workflow manually with `promote_to=production` and `source_tag=stag` to promote the same image to `prod` and `latest`.
- Configure required reviewers for both GitHub Environments `staging` and `production` to enforce approval before either promotion.

The workflow does not deploy to Azure because the Azure subscription has expired. The GHCR image tags are ready for a future deployment target. Keep database, OAuth, payment, email, and Cloudinary settings in local `.env` files or a deployment secret manager, not in the repository.
