using Microsoft.EntityFrameworkCore;
using TSG_Commex_BE.Data;
using TSG_Commex_BE.Models.Domain;
using TSG_Commex_BE.Models.Enums;
using TSG_Commex_BE.Repositories.Interfaces;

namespace TSG_Commex_BE.Repositories.Implementations;

public class GlobalStatusRepository : IGlobalStatusRepository
{
    private readonly ApplicationDbContext _context;

    public GlobalStatusRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<GlobalStatus>> GetAllAsync()
    {
        return await _context.GlobalStatuses
            .Where(s => s.IsActive)
            .OrderBy(s => s.Phase)
            .ThenBy(s => s.StatusCode)
            .ToListAsync();
    }

    public async Task<GlobalStatus?> GetByIdAsync(int id)
    {
        return await _context.GlobalStatuses
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
    }

    public async Task<GlobalStatus?> GetByStatusCodeAsync(string statusCode)
    {
        return await _context.GlobalStatuses
            .FirstOrDefaultAsync(s => s.StatusCode == statusCode && s.IsActive);
    }

    public async Task<GlobalStatus> CreateAsync(GlobalStatus status)
    {
        _context.GlobalStatuses.Add(status);
        await _context.SaveChangesAsync();
        return status;
    }

    public async Task<GlobalStatus> UpdateAsync(GlobalStatus status)
    {
        _context.GlobalStatuses.Update(status);
        await _context.SaveChangesAsync();
        return status;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var status = await GetByIdAsync(id);
        if (status == null)
            return false;

        status.IsActive = false;
        _context.GlobalStatuses.Update(status);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<GlobalStatus>> GetByPhaseAsync(string phase)
    {
        if (!Enum.TryParse<StatusPhase>(phase, out var statusPhase))
            return Enumerable.Empty<GlobalStatus>();
            
        return await _context.GlobalStatuses
            .Where(s => s.Phase == statusPhase && s.IsActive)
            .OrderBy(s => s.StatusCode)
            .ToListAsync();
    }
}