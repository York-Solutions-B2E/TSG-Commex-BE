using TSG_Commex_Shared.DTOs.Request;
using TSG_Commex_Shared.DTOs.Response;

namespace TSG_Commex_BE.Services.Interfaces;

public interface IGlobalStatusService
{
    Task<IEnumerable<GlobalStatusResponse>> GetAllStatusesAsync();
    Task<GlobalStatusResponse?> GetStatusByIdAsync(int id);
    Task<GlobalStatusResponse> CreateStatusAsync(CreateGlobalStatusRequest request);
    Task<bool> UpdateStatusAsync(int id, UpdateGlobalStatusRequest request);
    Task<bool> DeleteStatusAsync(int id);

}