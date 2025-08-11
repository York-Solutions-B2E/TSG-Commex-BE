using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TSG_Commex_BE.Configuration;
using TSG_Commex_BE.DTOs.Events;
using TSG_Commex_BE.Services.Interfaces;
using Pastel;
using System.Drawing;

namespace TSG_Commex_BE.Services.Implementations;

public class RabbitMQConsumer : IRabbitMQConsumer, IDisposable
{
    private readonly RabbitMQSettings _settings;
    private readonly ILogger<RabbitMQConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IConnection? _connection;
    private IChannel? _channel;
    private string? _consumerTag;

    public RabbitMQConsumer(
        IOptions<RabbitMQSettings> settings,
        ILogger<RabbitMQConsumer> logger,
        IServiceProvider serviceProvider)
    {
        _settings = settings.Value;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                AutomaticRecoveryEnabled = _settings.AutomaticRecoveryEnabled,
                RequestedHeartbeat = TimeSpan.FromSeconds(_settings.RequestedHeartbeat)

            };
            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Ensure exchange and queue exist
            await _channel.ExchangeDeclareAsync(_settings.ExchangeName, ExchangeType.Topic, durable: true);
            await _channel.QueueDeclareAsync(_settings.QueueName, durable: true, exclusive: false, autoDelete: false);
            await _channel.QueueBindAsync(_settings.QueueName, _settings.ExchangeName, "communication.*");

            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                await ProcessMessage(ea);
            };
            _consumerTag = await _channel.BasicConsumeAsync(
                queue: _settings.QueueName,
                autoAck: false,
                // If the consumer dies mid-work, RabbitMQ requeues the message.
                consumer: consumer
            );


            var startedMessage = $"🎯 RabbitMQ Consumer started successfully! Listening on queue: {_settings.QueueName}".Pastel(Color.LimeGreen);
            _logger.LogInformation(startedMessage);
            Console.WriteLine($"{"[CONSUMER]".Pastel(Color.Magenta)} {startedMessage}");
        }
        catch (Exception ex)
        {
            var errorMessage = $"💥 Failed to start RabbitMQ consumer: {ex.Message}".Pastel(Color.Red);
            _logger.LogError(ex, errorMessage);
            Console.WriteLine($"{"[ERROR]".Pastel(Color.Red)} {errorMessage}");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_consumerTag != null && _channel != null)
            {
                await _channel.BasicCancelAsync(_consumerTag);
            }

            _channel?.Dispose();
            _connection?.Dispose();

            var stoppedMessage = "🛑 RabbitMQ Consumer stopped gracefully".Pastel(Color.Orange);
            _logger.LogInformation(stoppedMessage);
            Console.WriteLine($"{"[CONSUMER]".Pastel(Color.Magenta)} {stoppedMessage}");
        }
        catch (Exception ex)
        {
            var errorMessage = $"💥 Error stopping RabbitMQ consumer: {ex.Message}".Pastel(Color.Red);
            _logger.LogError(ex, errorMessage);
            Console.WriteLine($"{"[ERROR]".Pastel(Color.Red)} {errorMessage}");
            throw;
        }
    }

    private async Task ProcessMessage(BasicDeliverEventArgs ea)
    {
        try
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            var processingMessage = $"📨 Processing message with routing key: {ea.RoutingKey}".Pastel(Color.Cyan);
            _logger.LogInformation(processingMessage);
            Console.WriteLine($"{"[MESSAGE]".Pastel(Color.Yellow)} {processingMessage}");

            using var scope = _serviceProvider.CreateScope();
            var eventProcessingService = scope.ServiceProvider.GetRequiredService<IEventProcessingService>();

            var eventData = JsonSerializer.Deserialize<JsonDocument>(message);
            var eventType = eventData?.RootElement.GetProperty("eventType").GetString();

            var processed = false;

            switch (eventType)
            {
                case "CommunicationStatusChanged":
                    var statusChangedEvent = JsonSerializer.Deserialize<CommunicationStatusChangedEvent>(message);
                    if (statusChangedEvent != null)
                    {
                        await eventProcessingService.ProcessStatusChangedEventAsync(statusChangedEvent);
                        processed = true;
                    }
                    break;

                case "CommunicationCreated":
                    var createdEvent = JsonSerializer.Deserialize<CommunicationCreatedEvent>(message);
                    if (createdEvent != null)
                    {
                        await eventProcessingService.ProcessCommunicationCreatedEventAsync(createdEvent);
                        processed = true;
                    }
                    break;

                default:
                    var unknownMessage = $"❓ Unknown event type: {eventType}".Pastel(Color.Yellow);
                    _logger.LogWarning(unknownMessage);
                    Console.WriteLine($"{"[WARNING]".Pastel(Color.Yellow)} {unknownMessage}");
                    await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
                    return;
            }

            if (processed)
            {
                await _channel!.BasicAckAsync(ea.DeliveryTag, multiple: false);
                var successMessage = $"✅ Successfully processed event: {eventType}".Pastel(Color.Green);
                _logger.LogInformation(successMessage);
                Console.WriteLine($"{"[SUCCESS]".Pastel(Color.Green)} {successMessage}");
            }
            else
            {
                await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
                var failedMessage = $"❌ Failed to process event {eventType}, requeuing...".Pastel(Color.Red);
                _logger.LogWarning(failedMessage);
                Console.WriteLine($"{"[RETRY]".Pastel(Color.Orange)} {failedMessage}");
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"💥 Error processing RabbitMQ message: {ex.Message}".Pastel(Color.Red);
            _logger.LogError(ex, errorMessage);
            Console.WriteLine($"{"[ERROR]".Pastel(Color.Red)} {errorMessage}");
            await _channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}