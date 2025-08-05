using TSG_Commex_BE.Models.Domain;
using TSG_Commex_BE.Models.Enums;

namespace TSG_Commex_BE.Repositories.Interfaces;

public interface IGlobalStatusRepository
{
    Task<IEnumerable<GlobalStatus>> GetAllAsync();
    Task<GlobalStatus?> GetByIdAsync(int id);
    Task<GlobalStatus?> GetByStatusCodeAsync(string statusCode);
    Task<GlobalStatus> CreateAsync(GlobalStatus status);
    Task<GlobalStatus> UpdateAsync(GlobalStatus status);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<GlobalStatus>> GetByPhaseAsync(string phase);
}