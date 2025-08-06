using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TSG_Commex_BE.Services.Interfaces;
using TSG_Commex_Shared.DTOs.Request;
using TSG_Commex_Shared.DTOs.Response;

namespace TSG_Commex_BE.Controllers;

[ApiController]
[Route("api/global-statuses")]
[AllowAnonymous] // Or [Authorize(Roles = "Admin")] if you want admin-only
public class GlobalStatusesController : ControllerBase
{
    private readonly IGlobalStatusService _globalStatusService;
    private readonly ILogger<GlobalStatusesController> _logger;

    public GlobalStatusesController(
        IGlobalStatusService globalStatusService,
        ILogger<GlobalStatusesController> logger)
    {
        _globalStatusService = globalStatusService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GlobalStatusResponse>>> GetAllStatuses()
    {
        try
        {
            var statuses = await _globalStatusService.GetAllStatusesAsync();
            return Ok(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving global statuses");
            return StatusCode(500, new { message = "Error retrieving statuses" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GlobalStatusResponse>> GetStatusById(int id)
    {
        try
        {
            var status = await _globalStatusService.GetStatusByIdAsync(id);
            if (status == null)
                return NotFound(new { message = $"Status with ID {id} not found" });

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving status with ID: {Id}", id);
            return StatusCode(500, new { message = "Error retrieving status" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<GlobalStatusResponse>> CreateStatus([FromBody] CreateGlobalStatusRequest request)
    {
        try
        {
            var status = await _globalStatusService.CreateStatusAsync(request);
            return CreatedAtAction(nameof(GetStatusById), new { id = status.Id }, status);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business logic error creating global status");
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument creating global status");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating global status");
            return StatusCode(500, new { message = "Error creating status" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateGlobalStatusRequest request)
    {
        try
        {
            var result = await _globalStatusService.UpdateStatusAsync(id, request);
            if (!result)
                return NotFound(new { message = $"Status with ID {id} not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status with ID: {Id}", id);
            return StatusCode(500, new { message = "Error updating status" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStatus(int id)
    {
        try
        {
            var result = await _globalStatusService.DeleteStatusAsync(id);
            if (!result)
                return NotFound(new { message = $"Status with ID {id} not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting status with ID: {Id}", id);
            return StatusCode(500, new { message = "Error deleting status" });
        }
    }
}