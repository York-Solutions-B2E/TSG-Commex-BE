using Microsoft.EntityFrameworkCore;
using TSG_Commex_BE.Data;
using TSG_Commex_BE.Models.Domain;
using TSG_Commex_BE.Repositories.Interfaces;

namespace TSG_Commex_BE.Repositories.Implementations;

public class MemberRepository : IMemberRepository
{
    private readonly ApplicationDbContext _context;

    public MemberRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Member>> GetAllAsync()
    {
        return await _context.Members
            .OrderBy(m => m.LastName)
            .ThenBy(m => m.FirstName)
            .ToListAsync();
    }

    public async Task<Member?> GetByIdAsync(int id)
    {
        return await _context.Members
            .Include(m => m.Communications)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Member?> GetByMemberIdAsync(string memberId)
    {
        return await _context.Members
            .Include(m => m.Communications)
            .FirstOrDefaultAsync(m => m.MemberId == memberId);
    }

    public async Task<Member?> GetByEmailAsync(string email)
    {
        return await _context.Members
            .Include(m => m.Communications)
            .FirstOrDefaultAsync(m => m.Email.ToLower() == email.ToLower());
    }

    public async Task<Member> CreateAsync(Member member)
    {
        member.CreatedUtc = DateTime.UtcNow;
        member.LastUpdatedUtc = DateTime.UtcNow;
        
        _context.Members.Add(member);
        await _context.SaveChangesAsync();
        return member;
    }

    public async Task<Member> UpdateAsync(Member member)
    {
        member.LastUpdatedUtc = DateTime.UtcNow;
        
        _context.Members.Update(member);
        await _context.SaveChangesAsync();
        return member;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var member = await GetByIdAsync(id);
        if (member == null)
            return false;

        // Soft delete
        member.IsActive = false;
        member.LastUpdatedUtc = DateTime.UtcNow;
        
        _context.Members.Update(member);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RestoreAsync(int id)
    {
        var member = await _context.Members
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsActive);
        
        if (member == null)
            return false;

        member.IsActive = true;
        member.LastUpdatedUtc = DateTime.UtcNow;
        
        _context.Members.Update(member);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Member>> GetActiveAsync()
    {
        return await _context.Members
            .Where(m => m.IsActive)
            .OrderBy(m => m.LastName)
            .ThenBy(m => m.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Member>> GetInactiveAsync()
    {
        return await _context.Members
            .Where(m => !m.IsActive)
            .OrderBy(m => m.LastName)
            .ThenBy(m => m.FirstName)
            .ToListAsync();
    }
}