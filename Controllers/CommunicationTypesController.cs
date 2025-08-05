using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TSG_Commex_BE.Models.Domain;
using TSG_Commex_BE.Services.Interfaces;
using TSG_Commex_Shared.DTOs;

namespace TSG_Commex_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class CommunicationTypesController : ControllerBase
{
    private readonly ICommunicationTypeService _communicationTypeService;
    private readonly ILogger<CommunicationTypesController> _logger;

    public CommunicationTypesController(
        ICommunicationTypeService communicationTypeService,
        ILogger<CommunicationTypesController> logger)
    {
        _communicationTypeService = communicationTypeService;
        _logger = logger;
    }

    [HttpGet("{typeCode}/statuses")]
    public async Task<ActionResult<IEnumerable<CommunicationTypeStatusResponse>>> GetStatusesForType(string typeCode)
    {
        try
        {
            _logger.LogInformation("📋 Fetching statuses for communication type: {TypeCode}", typeCode);
            
            var statuses = await _communicationTypeService.GetStatusesForTypeAsync(typeCode);
            
            if (!statuses.Any())
            {
                _logger.LogWarning("⚠️ No statuses found for type: {TypeCode}", typeCode);
                return NotFound(new { message = $"No statuses found for communication type: {typeCode}" });
            }
            
            _logger.LogInformation("✅ Successfully retrieved {Count} statuses for type: {TypeCode}", statuses.Count(), typeCode);
            return Ok(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error retrieving statuses for communication type: {TypeCode}", typeCode);
            return StatusCode(500, new { message = "Error retrieving statuses", error = ex.Message });
        }
    }
}