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
    Task<bool> UpdateStatusAsync(int commId, string newStatus);
    Task<List<Communication>> GetByStatusAsync(string status);

    Task<List<Communication>> GetByTypeCode(string typeCode);
}