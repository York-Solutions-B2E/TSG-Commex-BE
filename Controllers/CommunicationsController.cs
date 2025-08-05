using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TSG_Commex_BE.Data;
using TSG_Commex_BE.Services.Interfaces;
using TSG_Commex_BE.DTOs.Response;
using TSG_Commex_BE.DTOs.Request;


namespace TSG_Commex_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Temporarily disabled auth for testing
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

    [HttpPost]
    public async Task<ActionResult<CommunicationResponse>> CreateCommunication([FromBody] CreateCommunicationRequest request)
    {
        try
        {
            _logger.LogInformation("📝 Creating new communication of type: {TypeCode}", request.TypeCode);

            var communication = await _communicationService.CreateCommunicationAsync(request);

            _logger.LogInformation("✅ Successfully created communication with ID: {Id}", communication.Id);
            return CreatedAtAction(nameof(GetCommunication), new { id = communication.Id }, communication);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating communication");
            return StatusCode(500, new { message = "Error creating communication", error = ex.Message });
        }
    }
}