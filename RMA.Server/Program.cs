using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using RMA.Server.Services;
using System.Text;
using Google.Cloud.Firestore;
using RMA.Server.Entities;
using Microsoft.Extensions.Caching.Memory;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();

// Configure Firebase JWT Authentication
var projectId = builder.Configuration["Firebase:ProjectId"];
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://securetoken.google.com/{projectId}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{projectId}",
            ValidateAudience = true,
            ValidAudience = projectId,
            ValidateLifetime = true
        };
    })
    .AddJwtBearer("Local", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "RMAServer",
            ValidAudience = "RMAServer",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("RMA_SongLinh_SecretKey_For_Local_Testing_Only_12345"))
        };
    });

// Configure Authorization to accept both Firebase and Local tokens
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme, "Local")
        .RequireAuthenticatedUser()
        .Build();
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorWasm",
        policy =>
        {
            policy.SetIsOriginAllowed(origin => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
});

// Initialize Firestore
var googleAppCreds = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
if (string.IsNullOrEmpty(googleAppCreds))
{
    var credentialPath = builder.Configuration["Firebase:ServiceAccountKeyPath"] ?? "serviceAccountKey.json";
    if (!File.Exists(credentialPath))
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("==========================================================================");
        Console.WriteLine("❌ LỖI KHỞI ĐỘNG: Không tìm thấy file credentials 'serviceAccountKey.json'!");
        Console.WriteLine($"Vị trí cần đặt file: {Path.GetFullPath(credentialPath)}");
        Console.WriteLine("--------------------------------------------------------------------------");
        Console.WriteLine("HƯỚNG DẪN LẤY FILE:");
        Console.WriteLine("1. Truy cập Firebase Console: https://console.firebase.google.com/");
        Console.WriteLine("2. Vào Cài đặt dự án (Project Settings) -> Tài khoản dịch vụ (Service Accounts).");
        Console.WriteLine("3. Nhấp chọn 'Tạo khóa riêng tư mới' (Generate new private key) để tải file JSON về.");
        Console.WriteLine("4. Đổi tên file đã tải thành 'serviceAccountKey.json' và đặt vào thư mục RMA.Server.");
        Console.WriteLine("==========================================================================");
        Console.ResetColor();
    }
    else
    {
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Path.GetFullPath(credentialPath));
    }
}
var firestoreDb = FirestoreDb.Create(projectId);
builder.Services.AddSingleton(firestoreDb);

// Register Repositories
builder.Services.AddScoped<FirestoreRepository<Customer>>(provider => new FirestoreRepository<Customer>(provider.GetRequiredService<FirestoreDb>(), "customers", provider.GetService<IMemoryCache>()));
builder.Services.AddScoped<FirestoreRepository<Device>>(provider => new FirestoreRepository<Device>(provider.GetRequiredService<FirestoreDb>(), "devices", provider.GetService<IMemoryCache>()));
builder.Services.AddScoped<FirestoreRepository<Vendor>>(provider => new FirestoreRepository<Vendor>(provider.GetRequiredService<FirestoreDb>(), "vendors", provider.GetService<IMemoryCache>()));
builder.Services.AddScoped<FirestoreRepository<Brand>>(provider => new FirestoreRepository<Brand>(provider.GetRequiredService<FirestoreDb>(), "brands", provider.GetService<IMemoryCache>()));
builder.Services.AddScoped<FirestoreRepository<Model>>(provider => new FirestoreRepository<Model>(provider.GetRequiredService<FirestoreDb>(), "models", provider.GetService<IMemoryCache>()));
builder.Services.AddScoped<FirestoreRepository<Category>>(provider => new FirestoreRepository<Category>(provider.GetRequiredService<FirestoreDb>(), "categories", provider.GetService<IMemoryCache>()));
builder.Services.AddScoped<FirestoreRepository<StatusMaster>>(provider => new FirestoreRepository<StatusMaster>(provider.GetRequiredService<FirestoreDb>(), "status_masters", provider.GetService<IMemoryCache>()));
builder.Services.AddScoped<FirestoreRepository<Location>>(provider => new FirestoreRepository<Location>(provider.GetRequiredService<FirestoreDb>(), "locations", provider.GetService<IMemoryCache>()));
builder.Services.AddScoped<FirestoreRepository<RmaTicket>>(provider => new FirestoreRepository<RmaTicket>(provider.GetRequiredService<FirestoreDb>(), "rma_tickets", provider.GetService<IMemoryCache>()));
builder.Services.AddScoped<FirestoreRepository<Attachment>>(provider => new FirestoreRepository<Attachment>(provider.GetRequiredService<FirestoreDb>(), "attachments", provider.GetService<IMemoryCache>()));
builder.Services.AddScoped<FirestoreRepository<StatusHistory>>(provider => new FirestoreRepository<StatusHistory>(provider.GetRequiredService<FirestoreDb>(), "status_histories", provider.GetService<IMemoryCache>()));
builder.Services.AddScoped<FirestoreRepository<SalesOrder>>(provider => new FirestoreRepository<SalesOrder>(provider.GetRequiredService<FirestoreDb>(), "sales_orders", provider.GetService<IMemoryCache>()));
builder.Services.AddScoped<FirestoreRepository<SystemSetting>>(provider => new FirestoreRepository<SystemSetting>(provider.GetRequiredService<FirestoreDb>(), "system_settings", provider.GetService<IMemoryCache>()));
builder.Services.AddScoped<FirestoreRepository<AuditLog>>(provider => new FirestoreRepository<AuditLog>(provider.GetRequiredService<FirestoreDb>(), "audit_logs", provider.GetService<IMemoryCache>()));
builder.Services.AddScoped<FirestoreRepository<UserAccount>>(provider => new FirestoreRepository<UserAccount>(provider.GetRequiredService<FirestoreDb>(), "users", provider.GetService<IMemoryCache>()));

// Firebase Cloud Messaging (FCM)
builder.Services.AddSingleton<IFcmService, FcmService>();
builder.Services.AddHostedService<RmaAlertBackgroundService>();

// OCR Service
builder.Services.AddScoped<GoogleVisionOcrService>();
builder.Services.AddScoped<TesseractOcrService>();
builder.Services.AddScoped<IOcrService, BarcodeAndOcrService>();

// PDF Service
builder.Services.AddScoped<IPdfService, RmaReceiptPdfService>();

// Firebase Metrics & Cost Dashboard Services
builder.Services.AddSingleton<MetricsCollectorService>();
builder.Services.AddHostedService<MetricsFlushBackgroundService>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowBlazorWasm");

app.UseMiddleware<RMA.Server.Middlewares.ApiMetricsMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<RMA.Server.Hubs.SalesHub>("/salesHub");

app.Run();
