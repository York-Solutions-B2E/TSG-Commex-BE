using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TSG_Commex_BE.Data;
using TSG_Commex_BE.Models.Domain;
using TSG_Commex_BE.Services.Interfaces;
using TSG_Commex_Shared.DTOs;
using TSG_Commex_Shared.DTOs.Request;
using TSG_Commex_Shared.DTOs.Response;

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

    // GET: api/communication-types
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommunicationTypeResponse>>> GetAllTypes()
    {
        try
        {
            _logger.LogInformation("Fetching all communication types");
            var types = await _communicationTypeService.GetAllTypesAsync();
            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving communication types");
            return StatusCode(500, new { message = "Error retrieving communication types" });
        }
    }

    // GET: api/communication-types/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CommunicationTypeResponse>> GetTypeById(int id)
    {
        try
        {
            var type = await _communicationTypeService.GetTypeByIdAsync(id);
            if (type == null)
            {
                return NotFound(new { message = $"Communication type with ID {id} not found" });
            }
            return Ok(type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving communication type");
            return StatusCode(500, new { message = "Error retrieving communication type" });
        }
    }

    // POST: api/communication-types
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CommunicationTypeResponse>> CreateType([FromBody] CreateCommunicationTypeRequest request)
    {
        try
        {
            var type = await _communicationTypeService.CreateTypeAsync(request);
            return CreatedAtAction(nameof(GetTypeById), new { id = type.Id }, type);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business logic error creating communication type");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating communication type");
            return StatusCode(500, new { message = "Error creating communication type" });
        }
    }

    // PUT: api/communication-types/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateType(int id, [FromBody] UpdateCommunicationTypeRequest request)
    {
        try
        {
            var result = await _communicationTypeService.UpdateTypeAsync(id, request);
            if (!result)
            {
                return NotFound(new { message = $"Communication type with ID {id} not found" });
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business logic error updating communication type");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating communication type");
            return StatusCode(500, new { message = "Error updating communication type" });
        }
    }

    // DELETE: api/communication-types/5
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteType(int id)
    {
        try
        {
            var result = await _communicationTypeService.DeleteTypeAsync(id);
            if (!result)
            {
                return NotFound(new { message = $"Communication type with ID {id} not found" });
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot delete communication type");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting communication type");
            return StatusCode(500, new { message = "Error deleting communication type" });
        }
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
        var typeStatuses = await context.CommunicationTypeStatuses
            .Include(cts => cts.CommunicationType)
            .Include(cts => cts.GlobalStatus)
            .ToListAsync();
        
        return Ok(new 
        {
            CommunicationTypes = communicationTypes.Select(ct => new { ct.Id, ct.TypeCode, ct.DisplayName }),
            GlobalStatuses = globalStatuses.Select(gs => new { gs.Id, gs.StatusCode, gs.DisplayName }),
            CommunicationTypeStatuses = typeStatuses.Select(cts => new { 
                CommunicationTypeId = cts.CommunicationTypeId,
                TypeCode = cts.CommunicationType?.TypeCode,
                GlobalStatusId = cts.GlobalStatusId,
                StatusCode = cts.GlobalStatus?.StatusCode,
                IsActive = cts.IsActive
            }),
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

        // Get the IDs for foreign keys
        var eobType = await context.CommunicationTypes.FirstAsync(ct => ct.TypeCode == "EOB");
        var eopType = await context.CommunicationTypes.FirstAsync(ct => ct.TypeCode == "EOP");
        var idCardType = await context.CommunicationTypes.FirstAsync(ct => ct.TypeCode == "ID_CARD");

        var createdStatus = await context.GlobalStatuses.FirstAsync(gs => gs.StatusCode == "Created");
        var readyForReleaseStatus = await context.GlobalStatuses.FirstAsync(gs => gs.StatusCode == "ReadyForRelease");
        var printedStatus = await context.GlobalStatuses.FirstAsync(gs => gs.StatusCode == "Printed");
        var shippedStatus = await context.GlobalStatuses.FirstAsync(gs => gs.StatusCode == "Shipped");
        var deliveredStatus = await context.GlobalStatuses.FirstAsync(gs => gs.StatusCode == "Delivered");
        var failedStatus = await context.GlobalStatuses.FirstAsync(gs => gs.StatusCode == "Failed");

        // Seed CommunicationTypeStatus - Admin-configured valid statuses per type
        var communicationTypeStatuses = new CommunicationTypeStatus[]
        {
            // EOB statuses (complex document workflow)
            new CommunicationTypeStatus { CommunicationTypeId = eobType.Id, GlobalStatusId = createdStatus.Id, IsActive = true, Description = "EOB document created" },
            new CommunicationTypeStatus { CommunicationTypeId = eobType.Id, GlobalStatusId = readyForReleaseStatus.Id, IsActive = true, Description = "EOB ready for release" },
            new CommunicationTypeStatus { CommunicationTypeId = eobType.Id, GlobalStatusId = printedStatus.Id, IsActive = true, Description = "EOB printed by vendor" },
            new CommunicationTypeStatus { CommunicationTypeId = eobType.Id, GlobalStatusId = shippedStatus.Id, IsActive = true, Description = "EOB shipped to member" },
            new CommunicationTypeStatus { CommunicationTypeId = eobType.Id, GlobalStatusId = deliveredStatus.Id, IsActive = true, Description = "EOB delivered to member" },
            new CommunicationTypeStatus { CommunicationTypeId = eobType.Id, GlobalStatusId = failedStatus.Id, IsActive = true, Description = "EOB processing failed" },
            
            // ID Card statuses (simpler workflow)
            new CommunicationTypeStatus { CommunicationTypeId = idCardType.Id, GlobalStatusId = createdStatus.Id, IsActive = true, Description = "Card design created" },
            new CommunicationTypeStatus { CommunicationTypeId = idCardType.Id, GlobalStatusId = printedStatus.Id, IsActive = true, Description = "Card printed" },
            new CommunicationTypeStatus { CommunicationTypeId = idCardType.Id, GlobalStatusId = shippedStatus.Id, IsActive = true, Description = "Card shipped" },
            new CommunicationTypeStatus { CommunicationTypeId = idCardType.Id, GlobalStatusId = deliveredStatus.Id, IsActive = true, Description = "Card delivered" },
            new CommunicationTypeStatus { CommunicationTypeId = idCardType.Id, GlobalStatusId = failedStatus.Id, IsActive = true, Description = "Card processing failed" },
            
            // EOP statuses (document workflow)
            new CommunicationTypeStatus { CommunicationTypeId = eopType.Id, GlobalStatusId = createdStatus.Id, IsActive = true, Description = "EOP document created" },
            new CommunicationTypeStatus { CommunicationTypeId = eopType.Id, GlobalStatusId = readyForReleaseStatus.Id, IsActive = true, Description = "EOP ready for release" },
            new CommunicationTypeStatus { CommunicationTypeId = eopType.Id, GlobalStatusId = printedStatus.Id, IsActive = true, Description = "EOP printed" },
            new CommunicationTypeStatus { CommunicationTypeId = eopType.Id, GlobalStatusId = deliveredStatus.Id, IsActive = true, Description = "EOP delivered" },
            new CommunicationTypeStatus { CommunicationTypeId = eopType.Id, GlobalStatusId = failedStatus.Id, IsActive = true, Description = "EOP processing failed" }
        };

        foreach (var typeStatus in communicationTypeStatuses)
        {
            context.CommunicationTypeStatuses.Add(typeStatus);
        }
        
        await context.SaveChangesAsync();

        return Ok(new { message = $"Successfully seeded {communicationTypeStatuses.Length} CommunicationTypeStatuses" });
    }
}