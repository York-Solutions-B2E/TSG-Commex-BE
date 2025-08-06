using TSG_Commex_BE.Models.Domain;

namespace TSG_Commex_BE.Repositories.Interfaces;

public interface IMemberRepository
{
    Task<IEnumerable<Member>> GetAllAsync();
    Task<Member?> GetByIdAsync(int id);
    Task<Member?> GetByMemberIdAsync(string memberId);
    Task<Member?> GetByEmailAsync(string email);
    Task<Member> CreateAsync(Member member);
    Task<Member> UpdateAsync(Member member);
    Task<bool> DeleteAsync(int id);
    Task<bool> RestoreAsync(int id);
    Task<IEnumerable<Member>> GetActiveAsync();
    Task<IEnumerable<Member>> GetInactiveAsync();
}