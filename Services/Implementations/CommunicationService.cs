using TSG_Commex_BE.Repositories.Interfaces;
using TSG_Commex_BE.Services.Interfaces;

namespace TSG_Commex_BE.Services.Implementations;

public class CommunicationService : ICommunicationService
{
    private readonly ICommunicationRepository _communicationRepository;
    private readonly IStatusTransitionRepository _statusTransitionRepository;

    public CommunicationService(
        ICommunicationRepository communicationRepository,
        IStatusTransitionRepository statusTransitionRepository)
    {
        _communicationRepository = communicationRepository;
        _statusTransitionRepository = statusTransitionRepository;
    }

    // TODO: Implement all interface methods
}