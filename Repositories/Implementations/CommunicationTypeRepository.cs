using Microsoft.EntityFrameworkCore;
using TSG_Commex_BE.Data;
using TSG_Commex_BE.Models.Domain;
using TSG_Commex_BE.Repositories.Interfaces;

namespace TSG_Commex_BE.Repositories.Implementations;

public class CommunicationTypeRepository : ICommunicationTypeRepository
{
    private readonly ApplicationDbContext _context;

    public CommunicationTypeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CommunicationType>> GetAllAsync()
    {
        return await _context.CommunicationTypes
            .Where(t => t.IsActive)
            .ToListAsync();
    }
    public async Task<CommunicationType?> GetByIdAsync(int id)
    {
        return await _context.CommunicationTypes
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
    }

    // TODO: Implement all interface methods
}