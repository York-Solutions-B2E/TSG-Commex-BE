using TSG_Commex_BE.Models.Domain;

namespace TSG_Commex_BE.Repositories.Interfaces;

public interface ICommunicationRepository
{
    // TODO: Add your method signatures here
    Task<Communication> CreateAsync(Communication communication);
    Task<Communication?> GetByIdAsync(int id);
    Task<IEnumerable<Communication>> GetAllAsync();
    Task<bool> UpdateAsync(Communication communication);

    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateStatusAsync(int commId, int newStatusId, string? notes = null, string? EventSource = null, int? userId = null);
    Task<List<Communication>> GetByStatusAsync(int statusId);

    Task<List<Communication>> GetByTypeId(int typeId);

    // Soft Delete Methods
    Task<bool> RestoreAsync(int id);
    Task<List<Communication>> GetDeletedAsync();
    Task<bool> HardDeleteAsync(int id); // For admin use only
}