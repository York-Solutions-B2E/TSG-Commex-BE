using TSG_Commex_BE.Models.Domain;

namespace TSG_Commex_BE.Repositories.Interfaces;

public interface ICommunicationTypeStatusRepository
{
    // Task<bool> IsValidStatusForTypeAsync(string typeCode, string statusCode);
    // Task<IEnumerable<string>> GetValidStatusesForTypeAsync(string typeCode);
    // Task<IEnumerable<GlobalStatus>> GetStatusesByPhaseAsync(string phase);
    Task<IEnumerable<CommunicationTypeStatus>> CreateStatusMappingsAsync(int typeId, List<int> statusIds);
    Task<IEnumerable<CommunicationTypeStatus>> UpdateStatusMappingsAsync(int typeId, List<int> statusIds);
    Task<IEnumerable<CommunicationTypeStatus>> GetStatusIdsForTypeAsync(int typeId);
    Task<bool> DeleteStatusMappingsForTypeAsync(int typeId);
    Task<bool> DeleteSingleStatusMappingAsync(int typeId, int statusId);
}