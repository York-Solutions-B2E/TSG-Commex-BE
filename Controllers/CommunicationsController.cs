using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TSG_Commex_BE.Services.Interfaces;
using TSG_Commex_BE.DTOs.Response;
using TSG_Commex_BE.DTOs.Request;


namespace TSG_Commex_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class CommunicationsController : ControllerBase
{
    private readonly ICommunicationService _communicationService;
    private readonly ILogger<CommunicationsController> _logger;

    public CommunicationsController(
        ICommunicationService communicationService,
        ILogger<CommunicationsController> logger)
    {
        _communicationService = communicationService;
        _logger = logger;
    }

    // TODO: Add your API endpoints here
    // GET, POST, PUT, DELETE for communications
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommunicationResponse>>> GetCommunications([FromQuery] GetCommunicationRequest request)
    {
        try
        {
            _logger.LogInformation("📋 Fetching communications");

            var communications = await _communicationService.GetAllCommunicationsAsync();

            _logger.LogInformation("✅ Successfully retrieved {Count} communications", communications.Count());
            return Ok(communications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error retrieving communications");
            return StatusCode(500, new { message = "Error retrieving communications", error = ex.Message });
        }
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<CommunicationResponse>> GetCommunication(int id)
    {
        try
        {
            _logger.LogInformation("📋 Fetching communication with ID: {Id}", id);

            var communication = await _communicationService.GetCommunicationByIdAsync(id);

            if (communication == null)
            {
                _logger.LogWarning("⚠️ Communication with ID {Id} not found", id);
                return NotFound(new { message = $"Communication with ID {id} not found" });
            }

            _logger.LogInformation("✅ Successfully retrieved communication {Id}", id);
            return Ok(communication);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error retrieving communication with ID: {Id}", id);
            return StatusCode(500, new { message = "Error retrieving communication", error = ex.Message });
        }
    }
}