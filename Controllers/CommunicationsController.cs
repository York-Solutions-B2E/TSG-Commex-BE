using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TSG_Commex_BE.Data;
using TSG_Commex_BE.Services.Interfaces;
using TSG_Commex_Shared.DTOs;
using TSG_Commex_Shared.DTOs.Request;


namespace TSG_Commex_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Temporarily allow anonymous for development
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
            _logger.LogInformation("📋 Fetching all communications (auth disabled for development)");

            // FOR DEVELOPMENT: Just return all communications
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
    
    [HttpGet("admin/all")]
    public async Task<ActionResult<IEnumerable<CommunicationResponse>>> GetAllCommunicationsAdmin()
    {
        try
        {
            _logger.LogInformation("📋 Admin endpoint: Fetching all communications");

            var communications = await _communicationService.GetAllCommunicationsAsync();

            _logger.LogInformation("✅ Admin endpoint: Successfully retrieved {Count} communications", communications.Count());
            return Ok(communications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error retrieving all communications");
            return StatusCode(500, new { message = "Error retrieving communications", error = ex.Message });
        }
    }
    
    [HttpGet("member/{memberId}")]
    public async Task<ActionResult<IEnumerable<CommunicationResponse>>> GetCommunicationsByMember(int memberId)
    {
        try
        {
            _logger.LogInformation("📋 Fetching communications for member {MemberId}", memberId);

            var communications = await _communicationService.GetByMemberIdAsync(memberId);

            _logger.LogInformation("✅ Successfully retrieved {Count} communications for member {MemberId}", 
                communications.Count(), memberId);
            return Ok(communications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error retrieving communications for member {MemberId}", memberId);
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
            _logger.LogInformation("📝 Creating new communication of type ID: {TypeId}", request.CommunicationTypeId);

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

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCommunication(int id, [FromBody] UpdateCommunicationRequest request)
    {
        try
        {
            _logger.LogInformation("📝 Updating communication with ID: {Id}", id);

            var result = await _communicationService.UpdateCommunicationAsync(id, request);

            if (!result)
            {
                _logger.LogWarning("⚠️ Communication with ID {Id} not found", id);
                return NotFound(new { message = $"Communication with ID {id} not found" });
            }

            _logger.LogInformation("✅ Successfully updated communication {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating communication with ID: {Id}", id);
            return StatusCode(500, new { message = "Error updating communication", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCommunication(int id)
    {
        try
        {
            _logger.LogInformation("🗑️ Deleting communication with ID: {Id}", id);

            var result = await _communicationService.DeleteCommunicationAsync(id);

            if (!result)
            {
                _logger.LogWarning("⚠️ Communication with ID {Id} not found", id);
                return NotFound(new { message = $"Communication with ID {id} not found" });
            }

            _logger.LogInformation("✅ Successfully deleted communication {Id}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error deleting communication with ID: {Id}", id);
            return StatusCode(500, new { message = "Error deleting communication", error = ex.Message });
        }
    }

    [HttpGet("{id}/status-history")]
    public async Task<ActionResult<IEnumerable<CommunicationStatusHistory>>> GetCommunicationStatusHistory(int id)
    {
        try
        {
            _logger.LogInformation("📋 Fetching status history for communication {Id}", id);

            var history = await _communicationService.GetCommunicationStatusHistoryAsync(id);

            if (history == null)
            {
                _logger.LogWarning("⚠️ Communication with ID {Id} not found", id);
                return NotFound(new { message = $"Communication with ID {id} not found" });
            }

            _logger.LogInformation("✅ Successfully retrieved {Count} status history records for communication {Id}", 
                history.Count(), id);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error retrieving status history for communication {Id}", id);
            return StatusCode(500, new { message = "Error retrieving status history", error = ex.Message });
        }
    }
}