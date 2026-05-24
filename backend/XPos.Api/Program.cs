using System.Data;
using System.Text;
using FluentMigrator.Runner;
using Npgsql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Dapper;
using XPos.Api.Authorization;
using XPos.Data.Migrations;
using XPos.Data.Repositories;
using XPos.Domain.Interfaces;
using XPos.Domain.Services;
using XPos.Api.Services;

SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());
DefaultTypeMap.MatchNamesWithUnderscores = true;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Async(writeTo => writeTo.Console(new JsonFormatter()))
    .WriteTo.Async(writeTo => writeTo.File(
        formatter: new JsonFormatter(),
        path: "logs/xpos-log-.json",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7))
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();

// Rate Limiting (Token Bucket) per IP Address
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("OrderCreationPolicy", httpContext =>
    {
        var ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() 
                        ?? httpContext.Connection.RemoteIpAddress?.ToString() 
                        ?? "anonymous";

        return RateLimitPartition.GetTokenBucketLimiter(ipAddress, _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 5,
            QueueLimit = 0,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            TokensPerPeriod = 5,
            AutoReplenishment = true
        });
    });

    options.AddPolicy("ReceiptUploadPolicy", httpContext =>
    {
        var ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() 
                        ?? httpContext.Connection.RemoteIpAddress?.ToString() 
                        ?? "anonymous";

        return RateLimitPartition.GetTokenBucketLimiter(ipAddress, _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 3,
            QueueLimit = 0,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            TokensPerPeriod = 3,
            AutoReplenishment = true
        });
    });
});

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing");
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

// Database Connection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Current User Context
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<XPos.Domain.Interfaces.ICurrentUserService, XPos.Api.Services.CurrentUserService>();
builder.Services.AddScoped<IAuditContextProvider, AuditContextProvider>();

// Core Repositories
builder.Services.AddScoped<ICmsRepository, CmsRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IProviderRepository, ProviderRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();
builder.Services.AddScoped<ISaleRepository, SaleRepository>();
builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
builder.Services.AddScoped<ITransferRepository, TransferRepository>();
builder.Services.AddScoped<ISaleReturnRepository, SaleReturnRepository>();
builder.Services.AddScoped<IPurchaseReturnRepository, PurchaseReturnRepository>();
builder.Services.AddScoped<IAdjustmentRepository, AdjustmentRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();
builder.Services.AddScoped<ISettingRepository, SettingRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IExpenseCategoryRepository, ExpenseCategoryRepository>();
builder.Services.AddScoped<IQuotationRepository, QuotationRepository>();
builder.Services.AddScoped<ICashRegisterRepository, CashRegisterRepository>();
builder.Services.AddScoped<ICashShiftRepository, CashShiftRepository>();
builder.Services.AddScoped<ICashTransactionRepository, CashTransactionRepository>();

// Services
builder.Services.AddScoped<UnitConversionService>();
builder.Services.AddScoped<ISaleService, SaleService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<ITransferService, TransferService>();
builder.Services.AddScoped<IReturnService, ReturnService>();
builder.Services.AddScoped<IAdjustmentService, AdjustmentService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IQuotationService, QuotationService>();
builder.Services.AddScoped<ICashShiftService, CashShiftService>();
builder.Services.AddHttpClient<SiatSoapService>();
builder.Services.AddScoped<XPos.Api.Filters.RequireOpenShiftFilter>();

// Admin Settings Repositories
builder.Services.AddScoped<IMailSettingsRepository, MailSettingsRepository>();
builder.Services.AddScoped<IPaymentGatewaySettingsRepository, PaymentGatewaySettingsRepository>();
builder.Services.AddScoped<ISmsSettingsRepository, SmsSettingsRepository>();

// Admin Settings Services
builder.Services.AddScoped<IMailSettingsService, MailSettingsService>();
builder.Services.AddScoped<IPaymentGatewaySettingsService, PaymentGatewaySettingsService>();
builder.Services.AddScoped<ISmsSettingsService, SmsSettingsService>();

// FluentMigrator Configuration
builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(builder.Configuration.GetConnectionString("DefaultConnection"))
        .ScanIn(typeof(M202605090001_CoreSchema).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());

var app = builder.Build();

// Run Migrations
using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    try
    {
        runner.MigrateUp();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration error: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Excepción no controlada capturada en Middleware (Error 500): {Message}", ex.Message);
        throw;
    }
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
