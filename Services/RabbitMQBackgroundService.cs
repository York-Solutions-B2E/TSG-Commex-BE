using TSG_Commex_BE.Services.Interfaces;
using Pastel;
using System.Drawing;

namespace TSG_Commex_BE.Services;

public class RabbitMQBackgroundService : BackgroundService
{
    private readonly ILogger<RabbitMQBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public RabbitMQBackgroundService(
        ILogger<RabbitMQBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var startupMessage = "🚀 RabbitMQ Background Service starting...".Pastel(Color.Cyan);
        _logger.LogInformation(startupMessage);
        Console.WriteLine($"{"[RabbitMQ]".Pastel(Color.Orange)} {startupMessage}");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var consumer = scope.ServiceProvider.GetRequiredService<IRabbitMQConsumer>();

            var startingMessage = "🔌 Starting RabbitMQ Consumer...".Pastel(Color.LimeGreen);
            _logger.LogInformation(startingMessage);
            Console.WriteLine($"{"[RabbitMQ]".Pastel(Color.Orange)} {startingMessage}");

            await consumer.StartAsync(stoppingToken);

            var readyMessage = "✅ RabbitMQ Consumer is READY and listening for events!".Pastel(Color.Green);
            _logger.LogInformation(readyMessage);
            Console.WriteLine($"{"[RabbitMQ]".Pastel(Color.Orange)} {readyMessage}");

            // Keep the service running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }

            var stoppingMessage = "🛑 Stopping RabbitMQ Consumer...".Pastel(Color.Yellow);
            _logger.LogInformation(stoppingMessage);
            Console.WriteLine($"{"[RabbitMQ]".Pastel(Color.Orange)} {stoppingMessage}");

            await consumer.StopAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            var cancelledMessage = "⚠️ RabbitMQ Background Service was cancelled".Pastel(Color.Yellow);
            _logger.LogInformation(cancelledMessage);
            Console.WriteLine($"{"[RabbitMQ]".Pastel(Color.Orange)} {cancelledMessage}");
        }
        catch (Exception ex)
        {
            var errorMessage = $"💥 RabbitMQ Background Service encountered an error: {ex.Message}".Pastel(Color.Red);
            _logger.LogError(ex, errorMessage);
            Console.WriteLine($"{"[ERROR]".Pastel(Color.Red)} {errorMessage}");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        var stoppingMessage = "🔄 RabbitMQ Background Service stopping...".Pastel(Color.Orange);
        _logger.LogInformation(stoppingMessage);
        Console.WriteLine($"{"[RabbitMQ]".Pastel(Color.Orange)} {stoppingMessage}");

        await base.StopAsync(cancellationToken);

        var stoppedMessage = "⏹️ RabbitMQ Background Service stopped".Pastel(Color.Gray);
        _logger.LogInformation(stoppedMessage);
        Console.WriteLine($"{"[RabbitMQ]".Pastel(Color.Orange)} {stoppedMessage}");
    }
}