using TSG_Commex_BE.Repositories.Interfaces;
using TSG_Commex_BE.Services.Interfaces;

namespace TSG_Commex_BE.Services.Implementations;

public class EventProcessingService : IEventProcessingService
{
    private readonly ICommunicationRepository _communicationRepository;
    private readonly IStatusTransitionRepository _statusTransitionRepository;

    public EventProcessingService(
        ICommunicationRepository communicationRepository,
        IStatusTransitionRepository statusTransitionRepository)
    {
        _communicationRepository = communicationRepository;
        _statusTransitionRepository = statusTransitionRepository;
    }

    // TODO: Implement all interface methods
}