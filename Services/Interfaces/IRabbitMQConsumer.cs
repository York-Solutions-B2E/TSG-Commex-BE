namespace TSG_Commex_BE.Services.Interfaces;

public interface IRabbitMQConsumer
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}