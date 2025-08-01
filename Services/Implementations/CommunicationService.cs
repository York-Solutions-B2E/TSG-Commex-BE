using TSG_Commex_BE.Repositories.Interfaces;
using TSG_Commex_BE.Services.Interfaces;

namespace TSG_Commex_BE.Services.Implementations;

public class CommunicationService : ICommunicationService
{
    private readonly ICommunicationRepository _communicationRepository;
    private readonly ICommunicationTypeStatusRepository _communicationTypeStatusRepository;

    public CommunicationService(
        ICommunicationRepository communicationRepository,
        ICommunicationTypeStatusRepository communicationTypeStatusRepository)
    {
        _communicationRepository = communicationRepository;
        _communicationTypeStatusRepository = communicationTypeStatusRepository;
    }

    // TODO: Implement all interface methods
}