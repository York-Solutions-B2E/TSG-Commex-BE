using Microsoft.EntityFrameworkCore;
using TSG_Commex_BE.Data;
using TSG_Commex_BE.Models.Domain;
using TSG_Commex_BE.Repositories.Interfaces;

namespace TSG_Commex_BE.Repositories.Implementations;

public class CommunicationTypeStatusRepository : ICommunicationTypeStatusRepository
{
    private readonly ApplicationDbContext _context;

    public CommunicationTypeStatusRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<CommunicationTypeStatus>> GetStatusesForCommunicationTypeAsync(string typeCode)
    {
        return await _context.CommunicationTypeStatuses
            .Where(cts => cts.CommunicationType.TypeCode == typeCode && cts.IsActive)
            .Include(cts => cts.GlobalStatus)
            .OrderBy(cts => cts.GlobalStatus.Phase)
            .ThenBy(cts => cts.GlobalStatus.DisplayName)
            .ToListAsync();

    }
    // private readonly ApplicationDbContext _context;

    // public CommunicationTypeStatusRepository(ApplicationDbContext context)
    // {
    //     _context = context;
    // }

    // public async Task<bool> IsValidStatusForTypeAsync(string typeCode, string statusCode)
    // {
    //     return await _context.CommunicationTypeStatuses
    //         .AnyAsync(cts => cts.TypeCode == typeCode && cts.StatusCode == statusCode);
    // }

    // public async Task<IEnumerable<string>> GetValidStatusesForTypeAsync(string typeCode)
    // {
    //     return await _context.CommunicationTypeStatuses
    //         .Where(cts => cts.TypeCode == typeCode)
    //         .Select(cts => cts.StatusCode)
    //         .ToListAsync();
    // }

    // public async Task<IEnumerable<GlobalStatus>> GetStatusesByPhaseAsync(string phase)
    // {
    //     return await _context.GlobalStatuses
    //         .Where(gs => gs.Phase.ToString() == phase && gs.IsActive == true)
    //         .OrderBy(gs => gs.DisplayName)
    //         .ToListAsync();
    // }
}