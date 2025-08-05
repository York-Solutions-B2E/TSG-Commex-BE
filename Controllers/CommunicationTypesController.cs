using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TSG_Commex_BE.Data;
using TSG_Commex_BE.Models.Domain;
using TSG_Commex_BE.Services.Interfaces;
using TSG_Commex_Shared.DTOs;

namespace TSG_Commex_BE.Controllers;

[ApiController]
[Route("api/communication-types")]
[AllowAnonymous] // Temporarily disabled auth for testing
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

    [HttpGet("debug/check-data")]
    public async Task<IActionResult> CheckData([FromServices] ApplicationDbContext context)
    {
        var communicationTypes = await context.CommunicationTypes.ToListAsync();
        var globalStatuses = await context.GlobalStatuses.ToListAsync();
        var typeStatuses = await context.CommunicationTypeStatuses.ToListAsync();
        
        return Ok(new 
        {
            CommunicationTypes = communicationTypes.Select(ct => new { ct.TypeCode, ct.DisplayName }),
            GlobalStatuses = globalStatuses.Select(gs => new { gs.StatusCode, gs.DisplayName }),
            CommunicationTypeStatuses = typeStatuses.Select(cts => new { cts.TypeCode, cts.StatusCode }),
            TotalCommunicationTypes = communicationTypes.Count,
            TotalGlobalStatuses = globalStatuses.Count,
            TotalTypeStatuses = typeStatuses.Count
        });
    }

    [HttpPost("debug/seed-type-statuses")]
    public async Task<IActionResult> SeedTypeStatuses([FromServices] ApplicationDbContext context)
    {
        // Check if already seeded
        if (await context.CommunicationTypeStatuses.AnyAsync())
        {
            return Ok(new { message = "CommunicationTypeStatuses already seeded" });
        }

        // Seed CommunicationTypeStatus - Admin-configured valid statuses per type
        var communicationTypeStatuses = new CommunicationTypeStatus[]
        {
            // EOB statuses (complex document workflow)
            new CommunicationTypeStatus { TypeCode = "EOB", StatusCode = "Created", Description = "EOB document created" },
            new CommunicationTypeStatus { TypeCode = "EOB", StatusCode = "ReadyForRelease", Description = "EOB ready for release" },
            new CommunicationTypeStatus { TypeCode = "EOB", StatusCode = "Printed", Description = "EOB printed by vendor" },
            new CommunicationTypeStatus { TypeCode = "EOB", StatusCode = "Shipped", Description = "EOB shipped to member" },
            new CommunicationTypeStatus { TypeCode = "EOB", StatusCode = "Delivered", Description = "EOB delivered to member" },
            new CommunicationTypeStatus { TypeCode = "EOB", StatusCode = "Failed", Description = "EOB processing failed" },
            
            // ID Card statuses (simpler workflow)
            new CommunicationTypeStatus { TypeCode = "ID_CARD", StatusCode = "Created", Description = "Card design created" },
            new CommunicationTypeStatus { TypeCode = "ID_CARD", StatusCode = "Printed", Description = "Card printed" },
            new CommunicationTypeStatus { TypeCode = "ID_CARD", StatusCode = "Shipped", Description = "Card shipped" },
            new CommunicationTypeStatus { TypeCode = "ID_CARD", StatusCode = "Delivered", Description = "Card delivered" },
            new CommunicationTypeStatus { TypeCode = "ID_CARD", StatusCode = "Failed", Description = "Card processing failed" },
            
            // EOP statuses (document workflow)
            new CommunicationTypeStatus { TypeCode = "EOP", StatusCode = "Created", Description = "EOP document created" },
            new CommunicationTypeStatus { TypeCode = "EOP", StatusCode = "ReadyForRelease", Description = "EOP ready for release" },
            new CommunicationTypeStatus { TypeCode = "EOP", StatusCode = "Printed", Description = "EOP printed" },
            new CommunicationTypeStatus { TypeCode = "EOP", StatusCode = "Delivered", Description = "EOP delivered" },
            new CommunicationTypeStatus { TypeCode = "EOP", StatusCode = "Failed", Description = "EOP processing failed" }
        };

        foreach (var typeStatus in communicationTypeStatuses)
        {
            context.CommunicationTypeStatuses.Add(typeStatus);
        }
        
        await context.SaveChangesAsync();

        return Ok(new { message = $"Successfully seeded {communicationTypeStatuses.Length} CommunicationTypeStatuses" });
    }
}