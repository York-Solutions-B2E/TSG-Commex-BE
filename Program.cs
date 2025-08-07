using Microsoft.EntityFrameworkCore;
using TSG_Commex_BE.Data;
using TSG_Commex_BE.Repositories.Interfaces;
using TSG_Commex_BE.Repositories.Implementations;
using TSG_Commex_BE.Services.Interfaces;
using TSG_Commex_BE.Services.Implementations;
using TSG_Commex_BE.Configuration;  // ← For RabbitMQSettings
using TSG_Commex_BE.Services;       // ← For RabbitMQBackgroundService
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Configure secrets management based on environment
if (builder.Environment.IsDevelopment())
{
    // Development: Use User Secrets (secure for local dev)
    builder.Configuration.AddUserSecrets<Program>();
}

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

// Configure JWT Bearer Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.Authority = $"{builder.Configuration["Okta:OktaDomain"]}/oauth2/default";
    options.Audience = "api://default";
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            // Map Okta groups to ASP.NET Core roles (REQUIRED for role-based authorization)
            var identity = context.Principal?.Identity as ClaimsIdentity;
            var groupsClaims = context.Principal?.Claims
                .Where(c => c.Type == "groups")
                .Select(c => c.Value)
                .ToList();
            
            foreach (var group in groupsClaims)
            {
                identity?.AddClaim(new Claim(ClaimTypes.Role, group));
            }

            return Task.CompletedTask;
        }
    };
});

// Configure Authorization policies
builder.Services.AddAuthorization(options =>
{
    // Default policy requires authentication
    options.FallbackPolicy = options.DefaultPolicy;

        // Admin-only policy (matching frontend role assignment)
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
    
    // User or Admin policy  
    options.AddPolicy("UserOrAdmin", policy =>
        policy.RequireRole("User", "Admin"));
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp",
        builder =>
        {
            builder.WithOrigins("https://localhost:7268", "http://localhost:7268", "https://localhost:7018", "http://localhost:7018")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
        });
});

// Add API services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Repositories
builder.Services.AddScoped<ICommunicationRepository, CommunicationRepository>();
builder.Services.AddScoped<ICommunicationTypeRepository, CommunicationTypeRepository>();
builder.Services.AddScoped<ICommunicationTypeStatusRepository, CommunicationTypeStatusRepository>();
builder.Services.AddScoped<IGlobalStatusRepository, GlobalStatusRepository>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();

// Register Services
builder.Services.AddScoped<ICommunicationService, CommunicationService>();
builder.Services.AddScoped<ICommunicationTypeService, CommunicationTypeService>();
builder.Services.AddScoped<IEventProcessingService, EventProcessingService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddScoped<IGlobalStatusService, GlobalStatusService>();

// RabbitMQ configuration
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));

// RabbitMQ services
builder.Services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();
builder.Services.AddScoped<IRabbitMQConsumer, RabbitMQConsumer>();

// Background service
builder.Services.AddHostedService<RabbitMQBackgroundService>();

var app = builder.Build();

// Log Okta configuration on startup
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Okta Configuration:");
logger.LogInformation("  OktaDomain: {OktaDomain}", builder.Configuration["Okta:OktaDomain"]);
logger.LogInformation("  Authority: {Authority}", $"{builder.Configuration["Okta:OktaDomain"]}/oauth2/default");
logger.LogInformation("  MetadataAddress: {MetadataAddress}", $"{builder.Configuration["Okta:OktaDomain"]}/oauth2/default/.well-known/openid-configuration");

// Test Okta connectivity on startup
_ = Task.Run(async () =>
{
    try
    {
        var httpClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
        
        var metadataUrl = $"{builder.Configuration["Okta:OktaDomain"]}/oauth2/default/.well-known/openid-configuration";
        logger.LogInformation("🔍 Testing connectivity to Okta metadata endpoint: {Url}", metadataUrl);
        
        var response = await httpClient.GetAsync(metadataUrl);
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            logger.LogInformation("✅ Successfully connected to Okta metadata endpoint");
            logger.LogDebug("Metadata response length: {Length} chars", content.Length);
        }
        else
        {
            logger.LogError("❌ Failed to connect to Okta metadata endpoint. Status: {Status}", response.StatusCode);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error connecting to Okta metadata endpoint: {Error}", ex.Message);
    }
});

// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Ensure database and tables are created
    context.Database.EnsureCreated();

    // Seed with initial data
    DbInitializer.Initialize(context);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowBlazorApp");

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map Controllers (CRITICAL - this was missing!)
app.MapControllers();

app.Run();
