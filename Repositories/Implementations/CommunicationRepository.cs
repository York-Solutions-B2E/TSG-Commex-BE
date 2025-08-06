using Microsoft.EntityFrameworkCore;
using TSG_Commex_BE.Data;
using TSG_Commex_BE.Models.Domain;
using TSG_Commex_BE.Repositories.Interfaces;
using Pastel;

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
            .Include(c => c.CommunicationType)
            .Include(c => c.CurrentStatus)
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
            .Include(c => c.CommunicationType)
            .Include(c => c.CurrentStatus)
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

    public async Task<bool> UpdateStatusAsync(int commId, int newStatusId, string? notes = null, string? EventSource = null, int? userId = null)
    {
        try
        {
            Console.WriteLine($"{"[REPO-START]".Pastel("#FF00FF")} {"UpdateStatusAsync called for Communication".Pastel("#FFFFFF")} {commId.ToString().Pastel("#00FFFF")} {"→ StatusId:".Pastel("#FFFF00")} {newStatusId.ToString().Pastel("#00FF00")}");
            
            var communication = await _context.Communications.FindAsync(commId);
            if (communication == null)
            {
                Console.WriteLine($"{"[REPO-ERROR]".Pastel("#FF0000")} {"Communication not found:".Pastel("#FF6666")} {commId}");
                return false;
            }

            Console.WriteLine($"{"[REPO-UPDATE]".Pastel("#00FFFF")} {"Updating communication status from ID".Pastel("#FFFFFF")} {communication.CurrentStatusId.ToString().Pastel("#FFA500")} {"to ID".Pastel("#FFFFFF")} {newStatusId.ToString().Pastel("#00FF00")}");
            communication.CurrentStatusId = newStatusId;
            communication.LastUpdatedUtc = DateTime.UtcNow;

            var historyEntry = new CommunicationStatusHistory
            {
                CommunicationId = commId,
                GlobalStatusId = newStatusId,
                OccurredUtc = DateTime.UtcNow,
                Notes = notes ?? "Status changed via event",
                EventSource = EventSource ?? "RabbitMQ",
                UpdatedByUserId = userId
            };
            
            Console.WriteLine($"{"[REPO-HISTORY]".Pastel("#FFFF00")} {"Creating history entry:".Pastel("#FFFFFF")}");
            Console.WriteLine($"  {"• CommunicationId:".Pastel("#00FFFF")} {historyEntry.CommunicationId}");
            Console.WriteLine($"  {"• GlobalStatusId:".Pastel("#00FFFF")} {historyEntry.GlobalStatusId.ToString().Pastel("#00FF00")}");
            Console.WriteLine($"  {"• Notes:".Pastel("#00FFFF")} {historyEntry.Notes.Pastel("#CCCCCC")}");
            Console.WriteLine($"  {"• EventSource:".Pastel("#00FFFF")} {historyEntry.EventSource.Pastel("#FFA500")}");
            Console.WriteLine($"  {"• UserId:".Pastel("#00FFFF")} {(historyEntry.UpdatedByUserId?.ToString() ?? "null").Pastel("#CCCCCC")}");
            
            _context.CommunicationStatusHistories.Add(historyEntry);
            Console.WriteLine($"{"[REPO-HISTORY]".Pastel("#FFFF00")} {"History entry added to context".Pastel("#00FF00")}");

            Console.WriteLine($"{"[REPO-SAVE]".Pastel("#00FFFF")} {"Calling SaveChangesAsync...".Pastel("#FFFFFF")}");
            var changes = await _context.SaveChangesAsync();
            Console.WriteLine($"{"[REPO-SUCCESS]".Pastel("#00FF00")} {"SaveChangesAsync completed! Changes saved:".Pastel("#FFFFFF")} {changes.ToString().Pastel("#00FF00")}");
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{"[REPO-EXCEPTION]".Pastel("#FF0000")} {"ERROR in UpdateStatusAsync:".Pastel("#FF0000")} {ex.Message.Pastel("#FF6666")}");
            Console.WriteLine($"{"[REPO-EXCEPTION]".Pastel("#FF0000")} {"Exception Type:".Pastel("#FF0000")} {ex.GetType().Name.Pastel("#FF6666")}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"{"[REPO-INNER-EX]".Pastel("#FF0000")} {"Inner Exception:".Pastel("#FF0000")} {ex.InnerException.Message.Pastel("#FF6666")}");
                if (ex.InnerException.InnerException != null)
                {
                    Console.WriteLine($"{"[REPO-INNER2-EX]".Pastel("#FF0000")} {"Inner Inner Exception:".Pastel("#FF0000")} {ex.InnerException.InnerException.Message.Pastel("#FF6666")}");
                }
            }
            Console.WriteLine($"{"[REPO-STACK]".Pastel("#FF0000")} {"Stack Trace:".Pastel("#FF0000")}\n{ex.StackTrace?.Pastel("#FF9999")}");
            return false;
        }
    }

    public async Task<List<Communication>> GetByStatusAsync(int statusId)
    {
        return await _context.Communications
            .Where(c => c.IsActive == true && c.CurrentStatusId == statusId) // Manual soft delete filter
            .Include(c => c.CommunicationType)
            .Include(c => c.CurrentStatus)
            .Include(c => c.CreatedByUser)
            .Include(c => c.LastUpdatedByUser)
            .OrderByDescending(c => c.LastUpdatedUtc)
            .ToListAsync();
    }

    public async Task<List<Communication>> GetByTypeId(int typeId)
    {
        return await _context.Communications
            .Where(c => c.IsActive == true && c.CommunicationTypeId == typeId) // Manual soft delete filter
            .Include(c => c.CommunicationType)
            .Include(c => c.CurrentStatus)
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
            .Include(c => c.CommunicationType)
            .Include(c => c.CurrentStatus)
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