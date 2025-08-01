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

// Add other services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// Your API endpoints here...

app.Run();
