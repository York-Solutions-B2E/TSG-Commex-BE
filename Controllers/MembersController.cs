using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TSG_Commex_Shared.DTOs;
using TSG_Commex_BE.Services.Interfaces;

namespace TSG_Commex_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MembersController : ControllerBase
{
    private readonly IMemberService _memberService;
    private readonly ILogger<MembersController> _logger;

    public MembersController(IMemberService memberService, ILogger<MembersController> logger)
    {
        _memberService = memberService;
        _logger = logger;
    }

    // GET: api/members
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetMembers()
    {
        try
        {
            var members = await _memberService.GetAllMembersAsync();
            return Ok(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching members");
            return StatusCode(500, new { error = "An error occurred while fetching members" });
        }
    }

    // GET: api/members/5
    [HttpGet("{id}")]
    public async Task<ActionResult<MemberDto>> GetMember(int id)
    {
        try
        {
            var member = await _memberService.GetMemberByIdAsync(id);
            if (member == null)
            {
                return NotFound(new { error = $"Member with ID {id} not found" });
            }
            return Ok(member);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching member {Id}", id);
            return StatusCode(500, new { error = "An error occurred while fetching the member" });
        }
    }

    // GET: api/members/active
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetActiveMembers()
    {
        try
        {
            var members = await _memberService.GetActiveMembersAsync();
            return Ok(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching active members");
            return StatusCode(500, new { error = "An error occurred while fetching active members" });
        }
    }

    // POST: api/members
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<MemberDto>> CreateMember(CreateMemberDto dto)
    {
        try
        {
            var member = await _memberService.CreateMemberAsync(dto);
            return CreatedAtAction(nameof(GetMember), new { id = member.Id }, member);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating member");
            return StatusCode(500, new { error = "An error occurred while creating the member" });
        }
    }

    // PUT: api/members/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateMember(int id, UpdateMemberDto dto)
    {
        try
        {
            var member = await _memberService.UpdateMemberAsync(id, dto);
            if (member == null)
            {
                return NotFound(new { error = $"Member with ID {id} not found" });
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating member {Id}", id);
            return StatusCode(500, new { error = "An error occurred while updating the member" });
        }
    }

    // DELETE: api/members/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteMember(int id)
    {
        try
        {
            var result = await _memberService.DeleteMemberAsync(id);
            if (!result)
            {
                return NotFound(new { error = $"Member with ID {id} not found" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting member {Id}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the member" });
        }
    }
}