using TSG_Commex_BE.Models.Domain;
using TSG_Commex_BE.Repositories.Interfaces;
using TSG_Commex_BE.Services.Interfaces;
using TSG_Commex_Shared.DTOs;
using TSG_Commex_Shared.DTOs.Request;
using TSG_Commex_Shared.DTOs.Response;
using Microsoft.EntityFrameworkCore;
using TSG_Commex_BE.Data;

namespace TSG_Commex_BE.Services.Implementations;

public class CommunicationTypeService : ICommunicationTypeService
{
    private readonly ICommunicationTypeRepository _communicationTypeRepository;
    private readonly ICommunicationTypeStatusRepository _communicationTypeStatusRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CommunicationTypeService> _logger;

    public CommunicationTypeService(
        ICommunicationTypeRepository communicationTypeRepository,
        ICommunicationTypeStatusRepository communicationTypeStatusRepository,
        ApplicationDbContext context,
        ILogger<CommunicationTypeService> logger)
    {
        _communicationTypeRepository = communicationTypeRepository;
        _communicationTypeStatusRepository = communicationTypeStatusRepository;
        _context = context;
        _logger = logger;
    }
    // CRUD Operations
    public async Task<IEnumerable<CommunicationTypeResponse>> GetAllTypesAsync()
    {
        var types = await _communicationTypeRepository.GetAllAsync();
        var responses = new List<CommunicationTypeResponse>();
        
        foreach (var type in types)
        {
            var response = await MapToResponseAsync(type);
            responses.Add(response);
        }
        
        return responses;
    }

    public async Task<CommunicationTypeResponse?> GetTypeByIdAsync(int id)
    {
        var type = await _communicationTypeRepository.GetByIdAsync(id);
        return type == null ? null : await MapToResponseAsync(type);
    }

    public async Task<CommunicationTypeResponse?> GetTypeByCodeAsync(string typeCode)
    {
        var type = await _communicationTypeRepository.GetByTypeCodeAsync(typeCode);
        return type == null ? null : await MapToResponseAsync(type);
    }

    public async Task<CommunicationTypeResponse> CreateTypeAsync(CreateCommunicationTypeRequest request)
    {
        var type = new CommunicationType
        {
            TypeCode = request.TypeCode,
            DisplayName = request.DisplayName,
            Description = request.Description,
            IsActive = true
        };

        var created = await _communicationTypeRepository.CreateAsync(type);
        
        // Add status mappings if provided
        if (request.StatusIds.Any())
        {
            await UpdateStatusMappingsForTypeAsync(created.TypeCode, request.StatusIds);
        }
        
        return await MapToResponseAsync(created);
    }

    public async Task<bool> UpdateTypeAsync(int id, UpdateCommunicationTypeRequest request)
    {
        var type = await _communicationTypeRepository.GetByIdAsync(id);
        if (type == null)
            return false;

        if (!string.IsNullOrEmpty(request.DisplayName))
            type.DisplayName = request.DisplayName;
        
        if (!string.IsNullOrEmpty(request.Description))
            type.Description = request.Description;
        
        if (request.IsActive.HasValue)
            type.IsActive = request.IsActive.Value;

        await _communicationTypeRepository.UpdateAsync(type);
        
        // Update status mappings if provided
        if (request.StatusIds != null)
        {
            await UpdateStatusMappingsForTypeAsync(type.TypeCode, request.StatusIds);
        }
        
        return true;
    }

    public async Task<bool> DeleteTypeAsync(int id)
    {
        return await _communicationTypeRepository.DeleteAsync(id);
    }

    // Status Mapping Operations
    public async Task<IEnumerable<int>> GetStatusIdsForTypeAsync(string typeCode)
    {
        var mappings = await _context.CommunicationTypeStatuses
            .Where(cts => cts.CommunicationType.TypeCode == typeCode && cts.IsActive)
            .Select(cts => cts.GlobalStatusId)
            .ToListAsync();
            
        return mappings;
    }

    public async Task<bool> UpdateStatusMappingsForTypeAsync(string typeCode, List<int> statusIds)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Get the type
            var type = await _communicationTypeRepository.GetByTypeCodeAsync(typeCode);
            if (type == null)
                return false;

            // Deactivate existing mappings
            var existingMappings = await _context.CommunicationTypeStatuses
                .Where(cts => cts.CommunicationTypeId == type.Id && cts.IsActive)
                .ToListAsync();
            
            foreach (var mapping in existingMappings)
            {
                mapping.IsActive = false;
            }

            // Add new mappings
            foreach (var statusId in statusIds.Distinct())
            {
                var existingMapping = existingMappings.FirstOrDefault(m => m.GlobalStatusId == statusId);
                if (existingMapping != null)
                {
                    existingMapping.IsActive = true;
                }
                else
                {
                    _context.CommunicationTypeStatuses.Add(new CommunicationTypeStatus
                    {
                        CommunicationTypeId = type.Id,
                        GlobalStatusId = statusId,
                        IsActive = true
                    });
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error updating status mappings for type {TypeCode}", typeCode);
            throw;
        }
    }

    // Keep the existing method for backward compatibility
    public async Task<IEnumerable<CommunicationTypeStatusResponse>> GetStatusesForTypeAsync(string typeCode)
    {
        var statuses = await _communicationTypeStatusRepository.GetStatusesForCommunicationTypeAsync(typeCode);
        
        return statuses.Select(cts => new CommunicationTypeStatusResponse
        {
            StatusCode = cts.GlobalStatus.StatusCode,
            DisplayName = cts.GlobalStatus.DisplayName,
            Description = cts.GlobalStatus.Description,
            Phase = cts.GlobalStatus.Phase.ToString(),
            TypeSpecificDescription = cts.Description
        });
    }

    // Helper method
    private async Task<CommunicationTypeResponse> MapToResponseAsync(CommunicationType type)
    {
        var statusIds = await GetStatusIdsForTypeAsync(type.TypeCode);
        
        return new CommunicationTypeResponse
        {
            Id = type.Id,
            TypeCode = type.TypeCode,
            DisplayName = type.DisplayName,
            Description = type.Description,
            IsActive = type.IsActive,
            AssignedStatusIds = statusIds.ToList()
        };
    }
}