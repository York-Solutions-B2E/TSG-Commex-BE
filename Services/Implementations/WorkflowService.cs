using TSG_Commex_BE.Repositories.Interfaces;
using TSG_Commex_BE.Services.Interfaces;

namespace TSG_Commex_BE.Services.Implementations;

public class WorkflowService : IWorkflowService
{
    private readonly IStatusTransitionRepository _statusTransitionRepository;
    private readonly IGlobalStatusRepository _globalStatusRepository;
    private readonly ICommunicationTypeRepository _communicationTypeRepository;

    public WorkflowService(
        IStatusTransitionRepository statusTransitionRepository,
        IGlobalStatusRepository globalStatusRepository,
        ICommunicationTypeRepository communicationTypeRepository)
    {
        _statusTransitionRepository = statusTransitionRepository;
        _globalStatusRepository = globalStatusRepository;
        _communicationTypeRepository = communicationTypeRepository;
    }

    // TODO: Implement all interface methods
    // This is where your ranking system will be implemented!
}