using System.Text;
using automobile_backend.Data;
using automobile_backend.InterFaces.IRepositories;
using automobile_backend.InterFaces.IRepository;
using automobile_backend.InterFaces.IServices;
using automobile_backend.Repositories;
using automobile_backend.Repository;
using automobile_backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe; // <-- FIX 1: Added this using statement

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

// Register all your application services and repositories
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

// Register Payment & Billing System
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Register Notification & Alerts
builder.Services.AddScoped<INotificationService, NotificationService>();

// Register AI Chatbot Service
builder.Services.AddScoped<IChatbotService, ChatbotService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:3000") // Frontend origin
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Required for cookies
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// *** UPDATED SWAGGER CONFIGURATION ***
builder.Services.AddSwaggerGen(options =>
{
    // Define the Bearer security scheme
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                      "Enter 'Bearer' [space] and then your token in the text input below. \r\n\r\n" +
                      "Example: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http, // Use Http for Bearer
        Scheme = "Bearer" // The scheme name
    });

    // Make Swagger UI use the Bearer scheme
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


// 2. Configure Authentication (UPDATED SECTION)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => 
{
    // ... (your existing JWT config is fine) ...
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
   
    // This is the dedicated path the Google middleware will listen on.
    options.CallbackPath = "/api/Auth/signin-google"; 
    
    options.SignInScheme = ExternalCookieAuthenticationScheme;

    // This ensures the correlation cookie is sent correctly
    options.CorrelationCookie.SameSite = SameSiteMode.Lax; 
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});


// 3. Configure Authorization Policies 
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Staff", policy => policy.RequireRole("Admin", "Employee"));
});

// Configure Stripe API Key
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];


// 4. Build the application
var app = builder.Build();

// 5. Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Keep commented for local http development

// This order is critical
app.UseCors(MyAllowSpecificOrigins); // <-- FIX 2: Use the named policy you defined above

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();
app.Run();