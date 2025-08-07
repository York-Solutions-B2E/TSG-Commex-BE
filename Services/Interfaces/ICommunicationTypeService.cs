using TSG_Commex_Shared.DTOs;
using TSG_Commex_Shared.DTOs.Request;
using TSG_Commex_Shared.DTOs.Response;

namespace TSG_Commex_BE.Services.Interfaces;

public interface ICommunicationTypeService
{
    // CRUD operations
    Task<IEnumerable<CommunicationTypeResponse>> GetAllTypesAsync();
    Task<CommunicationTypeResponse?> GetTypeByIdAsync(int id);
    Task<CommunicationTypeResponse?> GetTypeByCodeAsync(string typeCode);
    Task<CommunicationTypeResponse> CreateTypeAsync(CreateCommunicationTypeRequest request);
    Task<bool> UpdateTypeAsync(int id, UpdateCommunicationTypeRequest request);
    Task<bool> DeleteTypeAsync(int id);

    // Status mapping operations
    Task<IEnumerable<int>> GetStatusIdsForTypeAsync(int typeId);
    Task<bool> UpdateStatusMappingsForTypeAsync(int typeId, List<int> statusIds);
    Task<IEnumerable<CommunicationTypeStatusResponse>> GetStatusesForTypeAsync(int typeId);
}