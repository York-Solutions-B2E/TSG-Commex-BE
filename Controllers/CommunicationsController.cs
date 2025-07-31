using Microsoft.AspNetCore.Mvc;
using TSG_Commex_BE.Services.Interfaces;

namespace TSG_Commex_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommunicationsController : ControllerBase
{
    private readonly ICommunicationService _communicationService;

    public CommunicationsController(ICommunicationService communicationService)
    {
        _communicationService = communicationService;
    }

    // TODO: Add your API endpoints here
    // GET, POST, PUT, DELETE for communications
}