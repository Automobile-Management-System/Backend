// using System.Text;
// using automobile_backend.Interfaces.IRepositories;
// using automobile_backend.Interfaces.IServices;
// using automobile_backend.InterFaces.IRepositories;
// using automobile_backend.InterFaces.IRepository;
// using automobile_backend.InterFaces.IServices;
// using automobile_backend.Repositories;
// using automobile_backend.Repositories.Interfaces;
// using automobile_backend.Repository;
// using automobile_backend.Services;
// using automobile_backend.Services.Interfaces;
// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.IdentityModel.Tokens;
// using Microsoft.OpenApi.Models;
// using automobile_backend.InterFaces.IServices;
// using Stripe; 
// using QuestPDF.Infrastructure;

// var builder = WebApplication.CreateBuilder(args);
// var configuration = builder.Configuration;

// const string MyAllowSpecificOrigins = "AllowFrontend";
// const string ExternalCookieAuthenticationScheme = "ExternalCookie"; // For Google Auth External Cookie

// // 1. Add services to the container.

// var conn = builder.Configuration.GetConnectionString("DefaultConnection");
// if (string.IsNullOrWhiteSpace(conn))
// {
//     throw new InvalidOperationException("DefaultConnection not found in configuration.");
// }

// // Add DbContext
// builder.Services.AddDbContext<ApplicationDbContext>(options =>
//     options.UseSqlServer(conn));

// // ==> ADD THIS: Required to access HttpContext in services like AuthService
// builder.Services.AddHttpContextAccessor();

// // Register all your application services and repositories
// builder.Services.AddScoped<IAuthRepository, AuthRepository>();
// builder.Services.AddScoped<IAuthService, AuthService>();

// builder.Services.AddScoped<ICustomerDashboardRepository, CustomerDashboardRepository>();
// builder.Services.AddScoped<ICustomerDashboardService, CustomerDashboardService>();

// builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
// builder.Services.AddScoped<IAppointmentService, AppointmentService>();

// builder.Services.AddScoped<IModificationRequestRepository, ModificationRequestRepository>();
// builder.Services.AddScoped<IModificationRequestService, ModificationRequestService>();


// builder.Services.AddScoped<IEmployeeServiceWorkRepository, EmployeeServiceWorkRepository>();
// builder.Services.AddScoped<IEmployeeServiceWorkService, EmployeeServiceWorkService>();

// builder.Services.AddScoped<IEmployeeManagementWorkRepository, EmployeeManagementWorkRepository>();
// builder.Services.AddScoped<IEmployeeManagementWorkService, EmployeeManagementWorkService>();

// builder.Services.AddScoped<IUserManagementRepository, UserManagementRepository>();
// builder.Services.AddScoped<IUserManagementService, UserManagementService>();

// builder.Services.AddScoped<IServiceAnalyticsRepository, ServiceAnalyticsRepository>();
// builder.Services.AddScoped<IServiceAnalyticsService, ServiceAnalyticsService>();

// // Register Admin Dashboard
// builder.Services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
// builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();

// // Register Payment & Billing System
// builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
// builder.Services.AddScoped<IPaymentService, PaymentService>();

// // Register the new Invoice Service
// builder.Services.AddScoped<IInvoiceService, automobile_backend.Services.InvoiceService>();

// // Register Notification & Alerts
// builder.Services.AddScoped<INotificationService, NotificationService>();



// // Register AI Chatbot Service
// builder.Services.AddHttpClient<IChatbotService, ChatbotService>();

// // Register Service Progress functionality
// builder.Services.AddScoped<IServiceProgressRepository, ServiceProgressRepository>();
// builder.Services.AddScoped<IServiceProgressService, ServiceProgressService>();

// // Register ViewService functionality
// builder.Services.AddScoped<IViewServiceRepository, ViewServiceRepository>();
// builder.Services.AddScoped<IViewServiceService, ViewServiceService>();

// builder.Services.AddScoped<ICloudStorageService, CloudStorageService>();


// // Configure CORS
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy(name: MyAllowSpecificOrigins,
//         policy =>
//         {
//             policy.WithOrigins("http://localhost:3000") // Frontend origin
//                   .AllowAnyHeader()
//                   .AllowAnyMethod()
//                   .AllowCredentials(); // Required for cookies
//         });
// });

// builder.Services.AddControllers();
// builder.Services.AddEndpointsApiExplorer();

// // Configure Swagger to use JWT
// builder.Services.AddSwaggerGen(options =>
// {
//     options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
//     {
//         In = ParameterLocation.Header,
//         Name = "Authorization",
//         Type = SecuritySchemeType.ApiKey
//     });
// });


// // 2. Configure Authentication (UPDATED SECTION)
// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
// })
// .AddJwtBearer(options =>
// {
//     // ... (your existing JWT config is fine) ...
//     options.Events = new JwtBearerEvents
//     {
//         OnMessageReceived = context =>
//         {
//             context.Token = context.Request.Cookies["jwt-token"];
//             return Task.CompletedTask;
//         }
//     };
//     options.TokenValidationParameters = new TokenValidationParameters
//     {
//         ValidateIssuer = true,
//         ValidateAudience = true,
//         ValidateLifetime = true,
//         ValidateIssuerSigningKey = true,
//         ValidIssuer = configuration["Jwt:Issuer"],
//         ValidAudience = configuration["Jwt:Audience"],
//         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
//         ClockSkew = TimeSpan.Zero
//     };
// })
// .AddCookie(ExternalCookieAuthenticationScheme, options =>
// {
//     options.Cookie.SameSite = SameSiteMode.Lax;
//     options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
// })
// .AddGoogle(options =>
// {
//     options.ClientId = configuration["Google:ClientId"]!;
//     options.ClientSecret = configuration["Google:ClientSecret"]!;



//     // This is the dedicated path the Google middleware will listen on.
//     options.CallbackPath = "/api/Auth/signin-google";

//     options.SignInScheme = ExternalCookieAuthenticationScheme;

//     // This ensures the correlation cookie is sent correctly
//     options.CorrelationCookie.SameSite = SameSiteMode.Lax;
//     options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
// });


// // 3. Configure Authorization Policies 
// builder.Services.AddAuthorization(options =>
// {
//     options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
//     options.AddPolicy("Staff", policy => policy.RequireRole("Admin", "Employee"));

//     options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
//     options.AddPolicy("EmployeeOnly", policy => policy.RequireRole("Employee"));
// });

// // Configure Stripe API Key
// StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

// QuestPDF.Settings.License = LicenseType.Community;

// // 4. Build the application
// var app = builder.Build();

// // 5. Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// // app.UseHttpsRedirection(); // Keep commented for local http development
// app.UseCors(MyAllowSpecificOrigins);

// app.UseAuthentication();
// app.UseAuthorization();

// app.MapControllers();
// app.Run();

using System.Text;
using automobile_backend.Interfaces.IRepositories;
using automobile_backend.Interfaces.IServices;
using automobile_backend.InterFaces.IRepositories;
using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Repositories;
using automobile_backend.Repositories.Interfaces;
using automobile_backend.Repository;
using automobile_backend.Services;
using automobile_backend.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using QuestPDF.Infrastructure;
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

const string MyAllowSpecificOrigins = "AllowFrontend";
const string ExternalCookieAuthenticationScheme = "ExternalCookie"; // For Google Auth External Cookie

// 1. Add services to the container.
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(conn))
{
    throw new InvalidOperationException("DefaultConnection not found in configuration.");
}

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(conn));

// ==> ADD THIS: Required to access HttpContext in services like AuthService
builder.Services.AddHttpContextAccessor();

// --- REGISTER APPLICATION SERVICES & REPOSITORIES ---
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<ICustomerDashboardRepository, CustomerDashboardRepository>();
builder.Services.AddScoped<ICustomerDashboardService, CustomerDashboardService>();

builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

builder.Services.AddScoped<IModificationRequestRepository, ModificationRequestRepository>();
builder.Services.AddScoped<IModificationRequestService, ModificationRequestService>();

builder.Services.AddScoped<IEmployeeServiceWorkRepository, EmployeeServiceWorkRepository>();
builder.Services.AddScoped<IEmployeeServiceWorkService, EmployeeServiceWorkService>();

builder.Services.AddScoped<IEmployeeManagementWorkRepository, EmployeeManagementWorkRepository>();
builder.Services.AddScoped<IEmployeeManagementWorkService, EmployeeManagementWorkService>();

builder.Services.AddScoped<IUserManagementRepository, UserManagementRepository>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();

builder.Services.AddScoped<IServiceAnalyticsRepository, ServiceAnalyticsRepository>();
builder.Services.AddScoped<IServiceAnalyticsService, ServiceAnalyticsService>();

builder.Services.AddScoped<IAdminDashboardRepository, AdminDashboardRepository>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddScoped<IInvoiceService, automobile_backend.Services.InvoiceService>();

builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddHttpClient<IChatbotService, ChatbotService>();

builder.Services.AddScoped<IServiceProgressRepository, ServiceProgressRepository>();
builder.Services.AddScoped<IServiceProgressService, ServiceProgressService>();

builder.Services.AddScoped<IViewServiceRepository, ViewServiceRepository>();
builder.Services.AddScoped<IViewServiceService, ViewServiceService>();

builder.Services.AddScoped<ICloudStorageService, CloudStorageService>();

// --- NEW: WebSocket Notification Service ---
builder.Services.AddSingleton<WebSocketNotificationService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); 
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger to use JWT
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
});

// --- Authentication ---
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["jwt-token"];
            return Task.CompletedTask;
        }
    };
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidAudience = configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
        ClockSkew = TimeSpan.Zero
    };
})
.AddCookie(ExternalCookieAuthenticationScheme, options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
})
.AddGoogle(options =>
{
    options.ClientId = configuration["Google:ClientId"]!;
    options.ClientSecret = configuration["Google:ClientSecret"]!;
    options.CallbackPath = "/api/Auth/signin-google";
    options.SignInScheme = ExternalCookieAuthenticationScheme;
    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// --- Authorization Policies ---
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Staff", policy => policy.RequireRole("Admin", "Employee"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
    options.AddPolicy("EmployeeOnly", policy => policy.RequireRole("Employee"));
});

// Stripe API Key
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
QuestPDF.Settings.License = LicenseType.Community;

// --- BUILD APP ---
var app = builder.Build();

// --- Middleware Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // optional for local dev
app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();
app.UseWebSockets();

// --- WebSocket Endpoint ---
// app.Map("/ws/admin-notifications", async context =>
// {
//     if (context.WebSockets.IsWebSocketRequest)
//     {
//         var socket = await context.WebSockets.AcceptWebSocketAsync();
//         var wsService = context.RequestServices.GetRequiredService<WebSocketNotificationService>();
//         wsService.AddAdminSocket(socket);

//         var buffer = new byte[1024 * 4];
//         var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

//         while (!result.CloseStatus.HasValue)
//         {
//             result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
//         }

//         wsService.RemoveAdminSocket(socket);
//         await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
//     }
app.Map("/ws/admin-notifications", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = 400;
        return;
    }

    var socket = await context.WebSockets.AcceptWebSocketAsync();
    var wsService = context.RequestServices.GetRequiredService<WebSocketNotificationService>();
    wsService.AddAdminSocket(socket);

    var buffer = new byte[1024 * 4];

    try
    {
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                break;
            }

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                // Optional: handle received message here
                Console.WriteLine($"Received WS message: {message}");
            }
        }
    }
    catch (WebSocketException ex)
    {
        Console.WriteLine("WebSocket closed unexpectedly: " + ex.Message);
    }
    finally
    {
        wsService.RemoveAdminSocket(socket);
        if (socket.State != WebSocketState.Closed)
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
        socket.Dispose();
    }
});

app.MapControllers();
app.Run();
