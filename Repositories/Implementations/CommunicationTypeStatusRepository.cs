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

    public async Task<IEnumerable<CommunicationTypeStatus>> CreateStatusMappingsAsync(int typeId, List<int> statusIds)
    {
        var createdMappings = new List<CommunicationTypeStatus>();

        foreach (var statusId in statusIds)
        {
            var mapping = new CommunicationTypeStatus
            {
                CommunicationTypeId = typeId,
                GlobalStatusId = statusId,
                IsActive = true
            };

            _context.CommunicationTypeStatuses.Add(mapping);
            createdMappings.Add(mapping);
        }

        await _context.SaveChangesAsync();

        return createdMappings;
    }

    public async Task<IEnumerable<CommunicationTypeStatus>> UpdateStatusMappingsAsync(int typeId, List<int> statusIds)
    {
        // Get existing mappings
        var existingMappings = await _context.CommunicationTypeStatuses
            .Where(cts => cts.CommunicationTypeId == typeId)
            .ToListAsync();

        var existingStatusIds = existingMappings
            .Where(m => m.IsActive)
            .Select(m => m.GlobalStatusId)
            .ToHashSet();

        var newStatusIds = statusIds.ToHashSet();

        // Determine what to add, remove, and reactivate
        var toAdd = newStatusIds.Except(existingMappings.Select(m => m.GlobalStatusId));
        var toDeactivate = existingStatusIds.Except(newStatusIds);
        var toReactivate = newStatusIds.Intersect(
            existingMappings.Where(m => !m.IsActive).Select(m => m.GlobalStatusId)
        );

        // Deactivate removed mappings
        if (toDeactivate.Any())
        {
            foreach (var mapping in existingMappings.Where(m => toDeactivate.Contains(m.GlobalStatusId)))
            {
                mapping.IsActive = false;
            }
        }

        // Reactivate previously deactivated mappings
        if (toReactivate.Any())
        {
            foreach (var mapping in existingMappings.Where(m => toReactivate.Contains(m.GlobalStatusId)))
            {
                mapping.IsActive = true;
            }
        }

        // Add new mappings
        if (toAdd.Any())
        {
            foreach (var statusId in toAdd)
            {
                var mapping = new CommunicationTypeStatus
                {
                    CommunicationTypeId = typeId,
                    GlobalStatusId = statusId,
                    IsActive = true
                };
                _context.CommunicationTypeStatuses.Add(mapping);
            }
        }

        await _context.SaveChangesAsync();

        // Return all active mappings
        return await GetStatusIdsForTypeAsync(typeId);
    }

    public async Task<IEnumerable<CommunicationTypeStatus>> GetStatusIdsForTypeAsync(int typeId)
    {
        return await _context.CommunicationTypeStatuses
            .Where(cts => cts.CommunicationTypeId == typeId && cts.IsActive)
            .Include(cts => cts.GlobalStatus)
            .ToListAsync();
    }

    public async Task<bool> DeleteStatusMappingsForTypeAsync(int typeId)
    {
        var mappings = await _context.CommunicationTypeStatuses
            .Where(cts => cts.CommunicationTypeId == typeId)
            .ToListAsync();

        if (mappings.Count == 0)
            return false;

        _context.CommunicationTypeStatuses.RemoveRange(mappings);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteSingleStatusMappingAsync(int typeId, int statusId)
    {
        var mapping = await _context.CommunicationTypeStatuses
            .FirstOrDefaultAsync(cts =>
                cts.CommunicationTypeId == typeId &&
                cts.GlobalStatusId == statusId);

        if (mapping == null)
            return false;

        _context.CommunicationTypeStatuses.Remove(mapping);
        await _context.SaveChangesAsync();

        return true;
    }
}