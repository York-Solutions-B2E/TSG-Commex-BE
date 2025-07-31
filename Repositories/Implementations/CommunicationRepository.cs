using Microsoft.EntityFrameworkCore;
using TSG_Commex_BE.Data;
using TSG_Commex_BE.Models.Domain;
using TSG_Commex_BE.Repositories.Interfaces;

namespace TSG_Commex_BE.Repositories.Implementations;

public class CommunicationRepository : ICommunicationRepository
{
    private readonly ApplicationDbContext _context;

    public CommunicationRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<IEnumerable<Communication>> GetAllAsync()
    {
        return await _context.Communications.ToListAsync();

        //     return await _context.Communications
        // .Include(c => c.CommunicationType)
        // .OrderByDescending(c => c.LastUpdatedUtc)
        // .Skip((page - 1) * pageSize)
        // .Take(pageSize)
        // .ToListAsync();
    }

    public async Task<Communication> CreateAsync(Communication communication)
    {
        _context.Communications.Add(communication);
        await _context.SaveChangesAsync();
        return communication;
    }

    // TODO: Implement all interface methods
}