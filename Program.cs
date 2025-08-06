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

// Configure JWT Bearer Authentication manually (more reliable than AddOktaWebApi)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.Authority = $"{builder.Configuration["Okta:OktaDomain"]}/oauth2/default";
    options.Audience = "api://default";
    options.RequireHttpsMetadata = true;
    
    // Configure Token validation Parameters to accept groups in role based authorization
    options.TokenValidationParameters = new TokenValidationParameters
    {
        RoleClaimType = "role", // Tell .NET to treat "role" claims as roles for authorization
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.FromMinutes(2)
    };
    
    // In development, accept any SSL certificate AND log more details
    if (builder.Environment.IsDevelopment())
    {
        options.BackchannelHttpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        
        // Add more detailed logging
        options.IncludeErrorDetails = true;
        
        // Force refresh of discovery document
        options.RefreshOnIssuerKeyNotFound = true;
    }
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            
            // Extract user info from JWT claims
            var email = context.Principal?.FindFirst("email")?.Value;
            var name = context.Principal?.FindFirst("name")?.Value ?? 
                      context.Principal?.FindFirst("preferred_username")?.Value;
            
            logger.LogInformation("JWT validated for user: {Email}", email);

            // Log all claims for debugging
            logger.LogInformation("All JWT claims:");
            foreach (var claim in context.Principal?.Claims ?? Enumerable.Empty<Claim>())
            {
                logger.LogInformation("  {Type}: {Value}", claim.Type, claim.Value);
            }
            
            // Check for role in different possible claim types from Okta
            var existingRole = context.Principal?.FindFirst("role")?.Value;
            var groupsClaim = context.Principal?.FindFirst("groups")?.Value;
            var rolesArrayClaim = context.Principal?.FindFirst("roles")?.Value;
            
            logger.LogInformation("Existing role claim: {Role}", existingRole ?? "None");
            logger.LogInformation("Groups claim: {Groups}", groupsClaim ?? "None");
            logger.LogInformation("Roles array claim: {Roles}", rolesArrayClaim ?? "None");
            
            var additionalClaims = new List<Claim>();
            
            // Map Okta groups to roles if needed
            if (!string.IsNullOrEmpty(groupsClaim))
            {
                // Okta sends groups as JSON array like ["Admin", "User"]
                var groups = System.Text.Json.JsonSerializer.Deserialize<string[]>(groupsClaim);
                if (groups != null)
                {
                    foreach (var group in groups)
                    {
                        additionalClaims.Add(new Claim(ClaimTypes.Role, group));
                        logger.LogInformation("Added role claim from group: {Group}", group);
                    }
                }
            }
            
            // Also check if there's a single "Admin" group
            var adminGroupClaim = context.Principal?.Claims
                .Where(c => c.Type == "groups" && c.Value == "Admin")
                .FirstOrDefault();
            
            if (adminGroupClaim != null)
            {
                additionalClaims.Add(new Claim(ClaimTypes.Role, "Admin"));
                logger.LogInformation("Added Admin role from groups claim");
            }
            
            // DEVELOPMENT ONLY: Add admin role for specific test users
            var isDevelopment = context.HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment();
            if (isDevelopment)
            {
                var userName = context.Principal?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
                var adminTestUsers = new[] { "MandradeC@yorksolutions.net", "admin@test.com" };
                
                if (!string.IsNullOrEmpty(userName) && adminTestUsers.Contains(userName, StringComparer.OrdinalIgnoreCase))
                {
                    additionalClaims.Add(new Claim(ClaimTypes.Role, "Admin"));
                    logger.LogInformation("DEVELOPMENT: Added Admin role for test user: {User}", userName);
                    
                    // Also add a test member ID for development
                    additionalClaims.Add(new Claim("member_id", "1"));
                    logger.LogInformation("DEVELOPMENT: Added test member_id claim: 1");
                }
            }

            // If we have additional claims, create a new ClaimsIdentity
            if (additionalClaims.Any())
            {
                var identity = context.Principal?.Identity as ClaimsIdentity;
                identity?.AddClaims(additionalClaims);
            }

            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(context.Exception, "JWT authentication failed: {Error}", context.Exception.Message);
            
            // Log additional details in development
            if (builder.Environment.IsDevelopment())
            {
                logger.LogError("Token: {Token}", context.Request.Headers.Authorization.FirstOrDefault());
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
