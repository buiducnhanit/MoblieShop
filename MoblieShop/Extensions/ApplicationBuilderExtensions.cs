using CloudinaryDotNet;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using System.Globalization;
using MoblieShop.Data;
using MoblieShop.Hubs;
using MoblieShop.Interface;
using MoblieShop.Mappings;
using MoblieShop.Models;
using MoblieShop.Repository;
using MoblieShop.Service;
using MoblieShop.Service.Cloudinary;
using MoblieShop.Service.MailKit;
using MoblieShop.Service.MomoPayment;
using MoblieShop.Service.PayPal;
using MoblieShop.Service.ProductRecommendation;
using MoblieShop.Service.VNPayPayment;

namespace MoblieShop.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static WebApplicationBuilder ConfigureApplication(this WebApplicationBuilder builder)
        {
            var config = builder.Configuration;

            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(config.GetConnectionString("DefaultConnection"), sqlOptions =>
                sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders().AddDefaultUI();
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            builder.Services.AddControllersWithViews()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();
            builder.Services.AddControllers().AddJsonOptions(options =>
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve);

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(5122);
                if (!string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase))
                {
                    options.ListenAnyIP(7209, listenOptions => listenOptions.UseHttps());
                }
                options.Limits.MaxRequestBodySize = 100 * 1024 * 1024;
            });
            builder.Services.ConfigureApplicationCookie(option =>
            {
                option.LoginPath = "/Identity/Account/Login";
                option.LogoutPath = "/Identity/Account/Logout";
                option.LogoutPath = "/Identity/Account/AccesDenied";
            });
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var isContainer = string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase);
            var googleClientId = config["Authentication:Google:ClientId"];
            var googleClientSecret = config["Authentication:Google:ClientSecret"];
            if (!isContainer || (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret)))
            {
                builder.Services.AddAuthentication().AddGoogle(options =>
                {
                    options.ClientId = googleClientId;
                    options.ClientSecret = googleClientSecret;
                });
            }

            var facebookAppId = config["Authentication:Facebook:AppId"];
            var facebookAppSecret = config["Authentication:Facebook:AppSecret"];
            if (!isContainer || (!string.IsNullOrWhiteSpace(facebookAppId) && !string.IsNullOrWhiteSpace(facebookAppSecret)))
            {
                builder.Services.AddAuthentication().AddFacebook(options =>
                {
                    options.AppId = facebookAppId;
                    options.AppSecret = facebookAppSecret;
                    options.AccessDeniedPath = "/AccessDeniedPathInfo";
                });
            }
            builder.Services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.Configure<CloudinarySettings>(config.GetSection("Cloudinary"));
            builder.Services.AddSingleton<ICloudinary>(provider =>
            {
                var settings = provider.GetRequiredService<IOptions<CloudinarySettings>>().Value;
                return new Cloudinary(new Account(settings.CloudName, settings.ApiKey, settings.ApiSecret));
            });

            builder.Services.AddHangfire(configuration => configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer().UseDefaultTypeSerializer()
                .UseSqlServerStorage(config.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));
            builder.Services.AddHangfireServer();

            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IRecommendationRepository, RecommendationRepository>();
            builder.Services.AddScoped<IProductRecommendationRepository, ProductRecommendationRepository>();
            builder.Services.AddScoped<IPostCategoryRepository, PostCategoryRepository>();
            builder.Services.AddScoped<IPostRepository, PostRepository>();
            builder.Services.AddScoped<ICommentRepository, CommentRepository>();
            builder.Services.AddSingleton<IVnPayService, VnPayService>();
            builder.Services.AddScoped<IMomoPaymentService, MomoPaymentService>();
            builder.Services.AddScoped<IProductViewRepository, ProductViewRepository>();
            builder.Services.AddScoped<IProductViewService, ProductViewService>();
            builder.Services.AddScoped<IPayPalPaymentService, PayPalPaymentService>();
            builder.Services.AddScoped<RecommendationService>();
            builder.Services.AddScoped<ProductRecommendationService>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            builder.Services.AddAutoMapper(typeof(MappingProfile));
            builder.Services.AddSignalR();
            builder.Services.AddRazorPages();
            return builder;
        }

        public static WebApplication ConfigureApplicationPipeline(this WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("vi-VN"),
                SupportedCultures = new[] { "en-US", "vi-VN" }.Select(c => new CultureInfo(c)).ToList(),
                SupportedUICultures = new[] { "en-US", "vi-VN" }.Select(c => new CultureInfo(c)).ToList(),
                RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new QueryStringRequestCultureProvider { QueryStringKey = "culture", UIQueryStringKey = "ui-culture" },
                    new CookieRequestCultureProvider(),
                    new AcceptLanguageHeaderRequestCultureProvider()
                }
            });
            app.Use(async (context, next) =>
            {
                if (context.Request.Query.ContainsKey("culture"))
                {
                    var culture = context.Request.Query["culture"];
                    context.Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)), new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.AddYears(1), HttpOnly = true,
                            SameSite = SameSiteMode.Lax, Secure = context.Request.IsHttps
                        });
                }
                await next.Invoke();
            });
            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();
            return app;
        }

        public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase))
            {
                await dbContext.Database.EnsureCreatedAsync();
            }
            else
            {
                await dbContext.Database.MigrateAsync();
            }
        }

        public static WebApplication MapApplicationEndpoints(this WebApplication app)
        {
            app.MapRazorPages();
            app.MapControllerRoute("Admin", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            app.MapControllerRoute("adminChat", "Admin/Chat/{action=AdminChat}/{id?}", new { controller = "Chat", action = "AdminChat" });
            app.MapControllerRoute("userChat", "User/Chat/{action=UserChat}/{id?}", new { controller = "Chat", action = "UserChat" });
            app.MapHub<ChatHub>("/chathub");
            app.MapHub<PostHub>("/posthub");
            app.UseHangfireDashboard("/hangfire");
            return app;
        }
    }
}
