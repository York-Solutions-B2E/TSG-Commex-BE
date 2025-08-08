using TSG_Commex_BE.Models.Domain;

namespace TSG_Commex_BE.Repositories.Interfaces;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<User?> UpdateAsync(int id, User user);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<IEnumerable<User>> GetActiveUsersAsync();
}