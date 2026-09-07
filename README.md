# MoblieShop

Ứng dụng bán điện thoại xây dựng bằng ASP.NET Core MVC, Entity Framework Core, SQL Server, ASP.NET Identity và SignalR.

## Công nghệ

- .NET 8 / ASP.NET Core MVC
- Entity Framework Core với SQL Server
- ASP.NET Core Identity
- SignalR cho chat và cập nhật thời gian thực
- AutoMapper
- Docker và Docker Compose
- GitHub Actions và GitHub Container Registry (GHCR)

## Yêu cầu

- .NET SDK 8
- Docker Desktop
- Git
- SQL Server nếu chạy trực tiếp ngoài Docker

## Cấu trúc chính

```text
MoblieShop/
├── Areas/          # Admin và Identity
├── Controllers/    # MVC controllers
├── Data/           # DbContext và seed data
├── Extensions/     # Cấu hình application và middleware
├── Hubs/           # SignalR hubs
├── Interface/      # Các interface của service/repository
├── Migrations/     # EF Core migrations
├── Models/         # Entity models
├── Repository/     # Truy cập dữ liệu
├── Service/        # Logic nghiệp vụ và tích hợp bên ngoài
├── ViewModels/     # View models
└── Views/          # Razor views
```

## Chạy trực tiếp bằng .NET

Từ thư mục chứa solution:

```powershell
dotnet restore .\MoblieShop\MoblieShop.csproj
dotnet build .\MoblieShop\MoblieShop.csproj
dotnet run --project .\MoblieShop\MoblieShop.csproj
```

Ứng dụng chạy mặc định tại `http://localhost:5122` và `https://localhost:7209` khi dùng profile HTTPS.

Cấu hình môi trường:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project .\MoblieShop\MoblieShop.csproj
```

Các file cấu hình được nạp theo thứ tự:

1. `appsettings.json`
2. `appsettings.{ASPNETCORE_ENVIRONMENT}.json`
3. Environment variables

Các môi trường có sẵn:

- `Development`: `appsettings.Development.json`
- `Staging`: `appsettings.Staging.json`
- `Production`: `appsettings.Production.json`

## Chạy bằng Docker Compose

Không đặt password, API key hoặc secret trực tiếp trong Compose file. Tạo file env riêng cho môi trường cần chạy:

### Development

```powershell
Copy-Item .\.env.development.example .\.env.development
docker compose --env-file .\.env.development `
  -f .\MoblieShop\docker-compose.development.yml up --build -d
```

Mở `http://localhost:5122`.

### Staging

```powershell
Copy-Item .\.env.staging.example .\.env.staging
docker compose --env-file .\.env.staging `
  -f .\MoblieShop\docker-compose.staging.yml up --build -d
```

Mở `http://localhost:5123`.

### Production local

```powershell
Copy-Item .\.env.production.example .\.env.production
docker compose --env-file .\.env.production `
  -f .\MoblieShop\docker-compose.production.yml up --build -d
```

Mở `http://localhost:5124`.

Dừng môi trường:

```powershell
docker compose --env-file .\.env.development `
  -f .\MoblieShop\docker-compose.development.yml down
```

Mỗi môi trường có SQL Server container và volume dữ liệu riêng. Ứng dụng tự tạo schema database khi chạy trong container và seed tài khoản admin.

## Secret và cấu hình nhạy cảm

Các file sau chỉ là template và được phép commit:

- `.env.development.example`
- `.env.staging.example`
- `.env.production.example`

Không commit các file sau:

- `.env`
- `.env.development`
- `.env.staging`
- `.env.production`
- Certificate hoặc private key (`*.pfx`, `*.pem`)

Connection string dùng environment variable dạng .NET:

```text
ConnectionStrings__DefaultConnection=...
```

Tên biến này sẽ ghi đè `ConnectionStrings:DefaultConnection` trong JSON. API keys cho Google, Facebook, PayPal, MoMo, VNPay, Cloudinary, email và OpenAI nên được lưu trong secret manager hoặc GitHub/Azure environment secrets.

## CI/CD GitHub Actions

Workflow nằm tại `.github/workflows/master_mobile-shop.yml` và áp dụng nguyên tắc **build once, promote artifact**:

```text
feature/*
   |
   +-- Pull Request -> dev/stag/master -> CI
                                      |
                       merge vào dev  |
                                      v
                      build Docker image một lần
                                      |
                         sha-<commit> + dev
                                      |
             Run workflow: promote_to=staging
                                      |
                          approval: staging
                                      |
                                     stag
                                      |
             Run workflow: promote_to=production
                                      |
                         approval: production
                                      |
                              prod + latest
```

### CI

Pull Request vào `dev`, `stag` hoặc `master` sẽ chạy:

- Checkout source
- Setup .NET 8
- Restore dependencies
- Build Release
- Test
- Publish application artifact

### Build artifact

Push vào `dev` sau khi CI thành công sẽ:

- Build Docker image đúng một lần
- Push image `ghcr.io/<owner>/<repository>:sha-<commit>`
- Push alias `ghcr.io/<owner>/<repository>:dev`

Tag `sha-<commit>` là artifact bất biến dùng cho các bước promote sau đó.

### Promote staging thủ công

1. Vào **Actions** và chọn workflow `CI/CD MobileShop`.
2. Chọn **Run workflow**.
3. Chọn `promote_to: staging`.
4. Nhập `source_tag: sha-<commit-dev>`.
5. Chờ reviewer approve Environment `staging`.

Workflow chỉ pull image, gắn tag `stag` và push lại. Không build Docker image mới.

### Promote production thủ công

Sau khi kiểm thử staging:

1. Chọn **Run workflow**.
2. Chọn `promote_to: production`.
3. Nhập `source_tag: stag`.
4. Chờ reviewer approve Environment `production`.

Workflow gắn cùng image thành `prod` và `latest`, không build lại.

Cần tạo hai GitHub Environments là `staging` và `production`, sau đó thêm **Required reviewers** cho từng environment. Azure deployment hiện chưa bật.

## Kiểm tra local trước khi push

```powershell
dotnet test .\MoblieShop\MoblieShop.csproj --configuration Release --no-restore
docker build --check -f .\MoblieShop\Dockerfile .\MoblieShop
git diff --check
```

Kiểm tra Compose:

```powershell
docker compose --env-file .\.env.development `
  -f .\MoblieShop\docker-compose.development.yml config
```

## GitHub bị chặn HTTPS

Nếu `git push` báo không kết nối được `github.com:443`, có thể dùng SSH qua port 443:

```powershell
ssh-keygen -t ed25519 -C "email-github-cua-ban"
Get-Content $env:USERPROFILE\.ssh\id_ed25519.pub | Set-Clipboard
git remote set-url origin ssh://git@ssh.github.com:443/buiducnhanit/MoblieShop.git
ssh -T -p 443 git@ssh.github.com
git push origin dev
```

Thêm public key vào GitHub tại **Settings → SSH and GPG keys → New SSH key**. Không chia sẻ private key.

## Tài khoản admin mặc định

Khi database mới được tạo, ứng dụng seed tài khoản:

```text
Email: admin@example.com
Password: Password123!
```

Đổi mật khẩu này ngay khi sử dụng ngoài môi trường local.
