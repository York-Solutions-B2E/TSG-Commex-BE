using TSG_Commex_BE.Models.Domain;
using TSG_Commex_BE.Repositories.Interfaces;
using TSG_Commex_BE.Services.Interfaces;
using TSG_Commex_Shared.DTOs;
using TSG_Commex_Shared.DTOs.Request;
using TSG_Commex_Shared.DTOs.Response;

namespace TSG_Commex_BE.Services.Implementations;

public class CommunicationTypeService : ICommunicationTypeService
{
    private readonly ICommunicationTypeRepository _communicationTypeRepository;
    private readonly ICommunicationTypeStatusRepository _communicationTypeStatusRepository;
    private readonly ILogger<CommunicationTypeService> _logger;

    public CommunicationTypeService(
        ICommunicationTypeRepository communicationTypeRepository,
        ICommunicationTypeStatusRepository communicationTypeStatusRepository,
        ILogger<CommunicationTypeService> logger)
    {
        _communicationTypeRepository = communicationTypeRepository;
        _communicationTypeStatusRepository = communicationTypeStatusRepository;
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
            await UpdateStatusMappingsForTypeAsync(created.Id, request.StatusIds);
        }
        
        return await MapToResponseAsync(created);
    }

    public async Task<bool> UpdateTypeAsync(int id, UpdateCommunicationTypeRequest request)
    {
        var type = await _communicationTypeRepository.GetByIdAsync(id);
        if (type == null)
            return false;

        // Store old TypeCode in case we need to update status mappings
        var oldTypeCode = type.TypeCode;

        if (!string.IsNullOrEmpty(request.TypeCode))
        {
            // Check if new TypeCode is already in use by another type
            var existingType = await _communicationTypeRepository.GetByTypeCodeAsync(request.TypeCode);
            if (existingType != null && existingType.Id != id)
            {
                throw new InvalidOperationException($"Communication type with code '{request.TypeCode}' already exists");
            }
            type.TypeCode = request.TypeCode;
        }

        if (!string.IsNullOrEmpty(request.DisplayName))
            type.DisplayName = request.DisplayName;
        
        if (!string.IsNullOrEmpty(request.Description))
            type.Description = request.Description;
        
        if (request.IsActive.HasValue)
            type.IsActive = request.IsActive.Value;

        await _communicationTypeRepository.UpdateAsync(type);
        
        // Update status mappings if provided
        // Update status mappings if provided
        if (request.StatusIds != null)
        {
            await UpdateStatusMappingsForTypeAsync(id, request.StatusIds);
        }
        
        return true;
    }

    public async Task<bool> DeleteTypeAsync(int id)
    {
        return await _communicationTypeRepository.DeleteAsync(id);
    }

    // Status Mapping Operations
    public async Task<IEnumerable<int>> GetStatusIdsForTypeAsync(int typeId)
    {
        // Get the status mappings for this type
        var mappings = await _communicationTypeStatusRepository.GetStatusIdsForTypeAsync(typeId);
        
        // Extract just the status IDs
        return mappings.Select(m => m.GlobalStatusId).ToList();
    }

    public async Task<bool> UpdateStatusMappingsForTypeAsync(int typeId, List<int> statusIds)
    {
        try
        {
            // Use repository to update mappings
            await _communicationTypeStatusRepository.UpdateStatusMappingsAsync(typeId, statusIds);
            
            _logger.LogInformation("Updated status mappings for type {TypeId}", typeId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status mappings for type {TypeId}", typeId);
            return false;
        }
    }

    // Get the statuses for a type
    public async Task<IEnumerable<CommunicationTypeStatusResponse>> GetStatusesForTypeAsync(int typeId)
    {
        var statuses = await _communicationTypeStatusRepository.GetStatusIdsForTypeAsync(typeId);
        
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
        var statusIds = await GetStatusIdsForTypeAsync(type.Id);
        
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