using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TSG_Commex_BE.Services.Interfaces;

namespace TSG_Commex_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all endpoints
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowService _workflowService;

    public WorkflowController(IWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    // TODO: Add your workflow management endpoints here
    // This is where your ranking system API will go!
}