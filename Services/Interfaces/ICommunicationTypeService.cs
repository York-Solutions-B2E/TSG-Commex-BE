using TSG_Commex_Shared.DTOs;

namespace TSG_Commex_BE.Services.Interfaces;

public interface ICommunicationTypeService
{
    Task<IEnumerable<CommunicationTypeStatusResponse>> GetStatusesForTypeAsync(string typeCode);
}