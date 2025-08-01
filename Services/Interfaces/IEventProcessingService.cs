using TSG_Commex_BE.DTOs.Events;
namespace TSG_Commex_BE.Services.Interfaces;

public interface IEventProcessingService
{
    Task ProcessStatusChangedEventAsync(CommunicationStatusChangedEvent eventData);
    Task ProcessCommunicationCreatedEventAsync(CommunicationCreatedEvent eventData);
    Task<bool> ValidateStatusTransitionAsync(int communicationId, string newStatus);
}