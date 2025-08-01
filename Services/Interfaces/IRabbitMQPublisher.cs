// TSG-Commex-BE/Services/Interfaces/IRabbitMQPublisher.cs
using TSG_Commex_BE.DTOs.Events;

namespace TSG_Commex_BE.Services.Interfaces;

public interface IRabbitMQPublisher
{
    Task PublishAsync<T>(T eventData, string routingKey = "") where T : class;
    Task PublishCommunicationStatusChangedAsync(CommunicationStatusChangedEvent eventData);
    Task PublishCommunicationCreatedAsync(CommunicationCreatedEvent eventData);
}