
using TSG_Commex_BE.Models.Domain;

namespace TSG_Commex_BE.Repositories.Interfaces;

public interface ICommunicationTypeRepository
{
    Task<IEnumerable<CommunicationType>> GetAllAsync();
    Task<CommunicationType?> GetByIdAsync(int id);
    Task<CommunicationType?> GetByTypeCodeAsync(string typeCode);
    Task<CommunicationType> CreateAsync(CommunicationType type);
    Task<CommunicationType> UpdateAsync(CommunicationType type);
    Task<bool> DeleteAsync(int id);
}