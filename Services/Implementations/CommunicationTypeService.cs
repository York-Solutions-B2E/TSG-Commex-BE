using TSG_Commex_BE.Models.Domain;
using TSG_Commex_BE.Repositories.Interfaces;
using TSG_Commex_BE.Services.Interfaces;
using TSG_Commex_Shared.DTOs;

namespace TSG_Commex_BE.Services.Implementations;

public class CommunicationTypeService : ICommunicationTypeService
{
    private readonly ICommunicationTypeStatusRepository _communicationTypeStatusRepository;
    private readonly ILogger<CommunicationTypeService> _logger;

    public CommunicationTypeService(
        ICommunicationTypeStatusRepository communicationTypeStatusRepository,
        ILogger<CommunicationTypeService> logger)
    {
        _communicationTypeStatusRepository = communicationTypeStatusRepository;
        _logger = logger;
    }
    public async Task<IEnumerable<CommunicationTypeStatusResponse>> GetStatusesForTypeAsync(string typeCode)
    {
        var statuses = await _communicationTypeStatusRepository.GetStatusesForCommunicationTypeAsync(typeCode);
        
        return statuses.Select(cts => new CommunicationTypeStatusResponse
        {
            StatusCode = cts.StatusCode,
            DisplayName = cts.GlobalStatus.DisplayName,
            Description = cts.GlobalStatus.Description,
            Phase = cts.GlobalStatus.Phase.ToString(),
            TypeSpecificDescription = cts.Description
        });
    }
}