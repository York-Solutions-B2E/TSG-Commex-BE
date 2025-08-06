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

    public async Task<CommunicationType?> GetByTypeCodeAsync(string typeCode)
    {
        return await _context.CommunicationTypes
            .FirstOrDefaultAsync(t => t.TypeCode == typeCode && t.IsActive);
    }

    public async Task<CommunicationType> CreateAsync(CommunicationType type)
    {
        _context.CommunicationTypes.Add(type);
        await _context.SaveChangesAsync();
        return type;
    }

    public async Task<CommunicationType> UpdateAsync(CommunicationType type)
    {
        _context.CommunicationTypes.Update(type);
        await _context.SaveChangesAsync();
        return type;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var type = await GetByIdAsync(id);
        if (type == null)
            return false;

        type.IsActive = false;
        _context.CommunicationTypes.Update(type);
        await _context.SaveChangesAsync();
        return true;
    }
}