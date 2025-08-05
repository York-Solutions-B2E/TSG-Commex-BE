using TSG_Commex_BE.DTOs.Events;
using TSG_Commex_BE.Repositories.Interfaces;
using TSG_Commex_BE.Services.Interfaces;
using Pastel;
using System.Drawing;

namespace TSG_Commex_BE.Services.Implementations;

public class EventProcessingService : IEventProcessingService
{
    private readonly ICommunicationRepository _communicationRepository;
    private readonly ILogger<EventProcessingService> _logger;

    public EventProcessingService(
        ICommunicationRepository communicationRepository,
        ILogger<EventProcessingService> logger)
    {
        _communicationRepository = communicationRepository;
        _logger = logger;
    }

    public async Task ProcessStatusChangedEventAsync(CommunicationStatusChangedEvent eventData)
    {
        try
        {
            var processingMessage = $"⚙️ Processing status change for communication {eventData.CommunicationId} → {eventData.NewStatus}".Pastel(Color.Cyan);
            _logger.LogInformation(processingMessage);
            Console.WriteLine($"{"[PROCESSING]".Pastel(Color.Blue)} {processingMessage}");

            // Parse communication ID
            if (!int.TryParse(eventData.CommunicationId, out var communicationId))
            {
                var errorMessage = $"❌ Invalid communication ID format: {eventData.CommunicationId}".Pastel(Color.Red);
                _logger.LogError(errorMessage);
                Console.WriteLine($"{"[ERROR]".Pastel(Color.Red)} {errorMessage}");
                return;
            }

            var success = await _communicationRepository.UpdateStatusAsync(
                communicationId,
                eventData.NewStatus,
                eventData.Notes,      // Pass the notes from the event
                eventData.Source,     // Pass the source (e.g., "EventSimulator")
                null                  // No user ID for system events
);

            if (success)
            {
                var successMessage = $"✅ Successfully updated communication {communicationId} to status: {eventData.NewStatus}".Pastel(Color.Green);
                _logger.LogInformation(successMessage);
                Console.WriteLine($"{"[SUCCESS]".Pastel(Color.Green)} {successMessage}");
            }
            else
            {
                var failureMessage = $"❌ Failed to update communication {communicationId} to status: {eventData.NewStatus}".Pastel(Color.Red);
                _logger.LogError(failureMessage);
                Console.WriteLine($"{"[FAILURE]".Pastel(Color.Red)} {failureMessage}");
            }
        }
        catch (Exception ex)
        {
            var errorMessage = $"💥 Error processing status change for communication {eventData.CommunicationId}: {ex.Message}".Pastel(Color.Red);
            _logger.LogError(ex, errorMessage);
            Console.WriteLine($"{"[EXCEPTION]".Pastel(Color.Red)} {errorMessage}");
            throw;
        }
    }

    public async Task ProcessCommunicationCreatedEventAsync(CommunicationCreatedEvent eventData)
    {
        try
        {
            // Just log it - no action needed for demo
            var createdMessage = $"📝 Communication created event logged for {eventData.CommunicationId} of type: {eventData.TypeCode}".Pastel(Color.Yellow);
            _logger.LogInformation(createdMessage);
            Console.WriteLine($"{"[CREATED]".Pastel(Color.Yellow)} {createdMessage}");
        }
        catch (Exception ex)
        {
            var errorMessage = $"💥 Error processing communication created event for {eventData.CommunicationId}: {ex.Message}".Pastel(Color.Red);
            _logger.LogError(ex, errorMessage);
            Console.WriteLine($"{"[EXCEPTION]".Pastel(Color.Red)} {errorMessage}");
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
                var notFoundMessage = $"❓ Communication {communicationId} not found for status validation".Pastel(Color.Yellow);
                _logger.LogWarning(notFoundMessage);
                Console.WriteLine($"{"[WARNING]".Pastel(Color.Yellow)} {notFoundMessage}");
                return false;
            }

            // For demo: allow any status change (skip complex validation)
            var validationMessage = $"✅ Status transition validated for communication {communicationId}: {communication.CurrentStatus} → {newStatus}".Pastel(Color.Green);
            _logger.LogInformation(validationMessage);
            Console.WriteLine($"{"[VALIDATION]".Pastel(Color.Green)} {validationMessage}");

            return true;
        }
        catch (Exception ex)
        {
            var errorMessage = $"💥 Error validating status transition for communication {communicationId} to status {newStatus}: {ex.Message}".Pastel(Color.Red);
            _logger.LogError(ex, errorMessage);
            Console.WriteLine($"{"[EXCEPTION]".Pastel(Color.Red)} {errorMessage}");
            return false;
        }
    }
}