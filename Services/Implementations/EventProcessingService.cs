using TSG_Commex_BE.DTOs.Events;
using TSG_Commex_BE.Repositories.Interfaces;
using TSG_Commex_BE.Services.Interfaces;

namespace TSG_Commex_BE.Services.Implementations;

public class EventProcessingService : IEventProcessingService
{
    private readonly ICommunicationRepository _communicationRepository;
    private readonly ILogger<EventProcessingService> _logger;

    public EventProcessingService(
        ILogger<EventProcessingService> logger)
    {
        _logger = logger;
    }

    public async Task ProcessStatusChangedEventAsync(CommunicationStatusChangedEvent eventData)
    {
        try
        {
            _logger.LogInformation("Processing status changed event for communication {CommunicationId} to status {NewStatus}",
                eventData.CommunicationId, eventData.NewStatus);

            // Parse communication ID
            if (!int.TryParse(eventData.CommunicationId, out var communicationId))
            {
                _logger.LogError("Invalid communication ID format: {CommunicationId}", eventData.CommunicationId);
                return;
            }

            // Use the existing UpdateStatusAsync method - this actually exists!
            var success = await _communicationRepository.UpdateStatusAsync(communicationId, eventData.NewStatus);

            if (success)
            {
                _logger.LogInformation("Successfully updated communication {CommunicationId} to status {NewStatus}",
                    communicationId, eventData.NewStatus);
            }
            else
            {
                _logger.LogError("Failed to update communication {CommunicationId} to status {NewStatus}",
                    communicationId, eventData.NewStatus);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing status changed event for communication {CommunicationId}",
                eventData.CommunicationId);
            throw;
        }
    }

    public async Task ProcessCommunicationCreatedEventAsync(CommunicationCreatedEvent eventData)
    {
        try
        {
            // Just log it - no action needed for demo
            _logger.LogInformation("Communication created event logged for {CommunicationId} of type {TypeCode}",
                eventData.CommunicationId, eventData.TypeCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing communication created event for communication {CommunicationId}",
                eventData.CommunicationId);
            throw;
        }
    }

    public async Task<bool> ValidateStatusTransitionAsync(int communicationId, string newStatus)
    {
        try
        {
            // Simplified: just check if the communication exists
            var communication = await _communicationRepository.GetByIdAsync(communicationId);

            if (communication == null)
            {
                _logger.LogWarning("Communication {CommunicationId} not found for status validation", communicationId);
                return false;
            }

            // For demo: allow any status change (skip complex validation)
            _logger.LogInformation("Status transition validated for communication {CommunicationId}: {CurrentStatus} -> {NewStatus}",
                communicationId, communication.CurrentStatus, newStatus);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating status transition for communication {CommunicationId} to status {NewStatus}",
                communicationId, newStatus);
            return false;
        }
    }
}