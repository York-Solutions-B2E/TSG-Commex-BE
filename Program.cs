using Microsoft.EntityFrameworkCore;
using TSG_Commex_BE.Data;
using TSG_Commex_BE.Repositories.Interfaces;
using TSG_Commex_BE.Repositories.Implementations;
using TSG_Commex_BE.Services.Interfaces;
using TSG_Commex_BE.Services.Implementations;
using TSG_Commex_BE.Configuration;  // ← For RabbitMQSettings
using TSG_Commex_BE.Services;       // ← For RabbitMQBackgroundService

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

// Register Services
builder.Services.AddScoped<ICommunicationService, CommunicationService>();
builder.Services.AddScoped<IEventProcessingService, EventProcessingService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();

// RabbitMQ configuration
builder.Services.Configure<RabbitMQSettings>(builder.Configuration.GetSection("RabbitMQ"));

// RabbitMQ services
builder.Services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();
builder.Services.AddScoped<IRabbitMQConsumer, RabbitMQConsumer>();

// Background service
builder.Services.AddHostedService<RabbitMQBackgroundService>();

var app = builder.Build();

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

// Map Controllers (CRITICAL - this was missing!)
app.MapControllers();

app.Run();
