using TSG_Commex_BE.Models.Domain;

namespace TSG_Commex_BE.Repositories.Interfaces;

public interface ICommunicationTypeStatusRepository
{
    // Task<bool> IsValidStatusForTypeAsync(string typeCode, string statusCode);
    // Task<IEnumerable<string>> GetValidStatusesForTypeAsync(string typeCode);
    // Task<IEnumerable<GlobalStatus>> GetStatusesByPhaseAsync(string phase);
    Task<IEnumerable<CommunicationTypeStatus>> GetStatusesForCommunicationTypeAsync(string typeCode);
}