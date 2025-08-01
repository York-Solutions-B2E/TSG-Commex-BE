using Microsoft.AspNetCore.Mvc;
using TSG_Commex_BE.DTOs.Events;
using TSG_Commex_BE.Services.Interfaces;

namespace TSG_Commex_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IRabbitMQPublisher _publisher;
    private readonly ILogger<EventsController> _logger;

    public EventsController(IRabbitMQPublisher publisher, ILogger<EventsController> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }
    [HttpPost("simulate-status-change")]
    public async Task<IActionResult> SimulateStatusChange([FromBody] SimulateStatusChangeRequest request)
    {
        try
        {
            var eventData = new CommunicationStatusChangedEvent
            {
                CommunicationId = request.CommunicationId,
                NewStatus = request.NewStatus,
                Source = "EventSimulator",
                Notes = request.Notes,
                Metadata = new Dictionary<string, string>
                {
                    { "simulatedBy", "EventSimulator" },
                    { "timestamp", DateTime.UtcNow.ToString("O") }
                }

            };
            await _publisher.PublishCommunicationStatusChangedAsync(eventData);

            return Ok(new { message = "Event published successfully", eventData });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish status change event");
            return StatusCode(500, new { message = "Failed to publish event", error = ex.Message });
        }

    }

    [HttpGet("test")]
    public async Task<IActionResult> QuickTest()
    {
        try
        {
            var testEvent = new CommunicationStatusChangedEvent
            {
                CommunicationId = "1",
                NewStatus = "Printed",
                Source = "QuickTest",
                Notes = "Testing from GET endpoint!",
                Metadata = new Dictionary<string, string>
                {
                    { "testType", "QuickTest" },
                    { "timestamp", DateTime.UtcNow.ToString("O") }
                }
            };

            await _publisher.PublishCommunicationStatusChangedAsync(testEvent);
            
            return Ok(new { 
                message = "🚀 Test event sent to RabbitMQ! Check your terminal logs!",
                eventData = testEvent 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send test event");
            return StatusCode(500, new { message = "Failed to send test event", error = ex.Message });
        }
    }
}
public class SimulateStatusChangeRequest
{
    public string CommunicationId { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string? Notes { get; set; }
}