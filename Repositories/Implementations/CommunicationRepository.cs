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
        return await _context.Communications
            .Where(c => c.IsActive == true) // Manual soft delete filter
            .Include(c => c.Type)
            .Include(c => c.CreatedByUser)
            .Include(c => c.LastUpdatedByUser)
            .OrderByDescending(c => c.LastUpdatedUtc)
            .ToListAsync();
    }

    public async Task<Communication> CreateAsync(Communication communication)
    {
        // Set creation timestamp
        communication.CreatedUtc = DateTime.UtcNow;
        communication.LastUpdatedUtc = DateTime.UtcNow;
        
        _context.Communications.Add(communication);
        await _context.SaveChangesAsync();
        return communication;
    }

    public async Task<Communication?> GetByIdAsync(int id)
    {
        return await _context.Communications
            .Where(c => c.IsActive == true) // Manual soft delete filter
            .Include(c => c.Type)
            .Include(c => c.CreatedByUser)
            .Include(c => c.LastUpdatedByUser)
            .Include(c => c.StatusHistory)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> UpdateAsync(Communication communication)
    {
        try
        {
            // Update the timestamp
            communication.LastUpdatedUtc = DateTime.UtcNow;
            
            _context.Communications.Update(communication);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var communication = await _context.Communications
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (communication == null)
            {
                return false;
            }

            // Soft delete: Set IsActive to false
            communication.IsActive = false;
            communication.LastUpdatedUtc = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateStatusAsync(int commId, string newStatus)
    {
        try
        {
            var communication = await _context.Communications.FindAsync(commId);
            if (communication == null)
            {
                return false;
            }

            communication.CurrentStatus = newStatus;
            communication.LastUpdatedUtc = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<Communication>> GetByStatusAsync(string status)
    {
        return await _context.Communications
            .Where(c => c.IsActive == true && c.CurrentStatus == status) // Manual soft delete filter
            .Include(c => c.Type)
            .Include(c => c.CreatedByUser)
            .Include(c => c.LastUpdatedByUser)
            .OrderByDescending(c => c.LastUpdatedUtc)
            .ToListAsync();
    }

    public async Task<List<Communication>> GetByTypeCode(string typeCode)
    {
        return await _context.Communications
            .Where(c => c.IsActive == true && c.TypeCode == typeCode) // Manual soft delete filter
            .Include(c => c.Type)
            .Include(c => c.CreatedByUser)
            .Include(c => c.LastUpdatedByUser)
            .OrderByDescending(c => c.LastUpdatedUtc)
            .ToListAsync();
    }

    // Soft Delete Utility Methods
    public async Task<bool> RestoreAsync(int id)
    {
        try
        {
            var communication = await _context.Communications
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive == false);

            if (communication == null)
            {
                return false; // Not found or already active
            }

            communication.IsActive = true;
            communication.LastUpdatedUtc = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<Communication>> GetDeletedAsync()
    {
        return await _context.Communications
            .Include(c => c.Type)
            .Include(c => c.CreatedByUser)
            .Include(c => c.LastUpdatedByUser)
            .Where(c => c.IsActive == false)
            .OrderByDescending(c => c.LastUpdatedUtc)
            .ToListAsync();
    }

    public async Task<bool> HardDeleteAsync(int id)
    {
        try
        {
            var communication = await _context.Communications
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (communication == null)
            {
                return false;
            }

            // PERMANENTLY delete (use with extreme caution!)
            _context.Communications.Remove(communication);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}