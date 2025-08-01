using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using TSG_Commex_BE.Configuration;
using TSG_Commex_BE.DTOs.Events;
using TSG_Commex_BE.Services.Interfaces;

namespace TSG_Commex_BE.Services.Implementations;

public class RabbitMQPublisher : IRabbitMQPublisher, IDisposable
{
    private readonly RabbitMQSettings _settings;
    private readonly ILogger<RabbitMQPublisher> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMQPublisher(IOptions<RabbitMQSettings> settings, ILogger<RabbitMQPublisher> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    private async Task EnsureConnectionAsync()
    {
        if (_connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen)
            return;

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

        await _channel.ExchangeDeclareAsync(_settings.ExchangeName, ExchangeType.Topic, durable: true);
        await _channel.QueueDeclareAsync(_settings.QueueName, durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueBindAsync(_settings.QueueName, _settings.ExchangeName, "communication.*");

        _logger.LogInformation("RabbitMQ connection established");
    }

    public async Task PublishAsync<T>(T eventData, string routingKey = "") where T : class
    {
        try
        {
            await EnsureConnectionAsync();

            var json = JsonSerializer.Serialize(eventData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            if (string.IsNullOrEmpty(routingKey))
            {
                routingKey = "communication.status.changed";
            }

            await _channel!.BasicPublishAsync(
                exchange: _settings.ExchangeName,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: properties,
                body: body
            );

            _logger.LogInformation("Published event {EventType} with routing key {RoutingKey}",
                typeof(T).Name, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType}", typeof(T).Name);
            throw;
        }
    }

    public async Task PublishCommunicationStatusChangedAsync(CommunicationStatusChangedEvent eventData)
    {
        await PublishAsync(eventData, "communication.status.changed");
    }

    public async Task PublishCommunicationCreatedAsync(CommunicationCreatedEvent eventData)
    {
        await PublishAsync(eventData, "communication.created");
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}