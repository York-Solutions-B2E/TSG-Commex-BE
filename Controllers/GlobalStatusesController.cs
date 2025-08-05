using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TSG_Commex_BE.Services.Interfaces;

namespace TSG_Commex_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class GlobalStatusesController : ControllerBase
{
    // TODO: Add constructor and endpoints
}