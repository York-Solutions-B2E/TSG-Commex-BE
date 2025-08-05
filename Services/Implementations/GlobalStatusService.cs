using Microsoft.EntityFrameworkCore;
using TSG_Commex_BE.Data;

using TSG_Commex_BE.Repositories.Interfaces;
using TSG_Commex_BE.Services.Interfaces;
using TSG_Commex_Shared.DTOs.Request;
using TSG_Commex_Shared.DTOs.Response;

namespace TSG_Commex_BE.Services.Implementations;

public class GlobalStatusService : IGlobalStatusService
{
    private readonly IGlobalStatusRepository _globalStatusRepository;
    private readonly ICommunicationTypeStatusRepository _communicationTypeStatusRepository;
    private readonly ILogger<GlobalStatusService> _logger;

    public GlobalStatusService(
        IGlobalStatusRepository globalStatusRepository,
        ICommunicationTypeStatusRepository communicationTypeStatusRepository,
        ILogger<GlobalStatusService> logger)
    {
        _globalStatusRepository = globalStatusRepository;
        _communicationTypeStatusRepository = communicationTypeStatusRepository;
        _logger = logger;
    }
    public async Task<IEnumerable<GlobalStatusResponse>> GetAllStatusesAsync()
    {
        var statuses = await _globalStatusRepository.GetAllAsync();

        return statuses.Select(MapToResponse);
    }

    public async Task<GlobalStatusResponse?> GetStatusByIdAsync(int id)
    {
        var status = await _globalStatusRepository.GetByIdAsync(id);
        if (status == null)
            return null;

        return MapToResponse(status);
    }


    public async Task<GlobalStatusResponse> CreateStatusAsync(CreateGlobalStatusRequest request)
    {
        var status = new Models.Domain.GlobalStatus
        {
            StatusCode = request.StatusCode,
            DisplayName = request.DisplayName,
            Description = request.Description,
            Phase = Enum.Parse<Models.Enums.StatusPhase>(request.Phase),
            IsActive = true
        };

        var created = await _globalStatusRepository.CreateAsync(status);
        return MapToResponse(created);
    }

    public async Task<bool> UpdateStatusAsync(int id, UpdateGlobalStatusRequest request)
    {
        var status = await _globalStatusRepository.GetByIdAsync(id);
        if (status == null)
            return false;

        if (!string.IsNullOrEmpty(request.DisplayName))
            status.DisplayName = request.DisplayName;
        
        if (!string.IsNullOrEmpty(request.Description))
            status.Description = request.Description;
        
        if (!string.IsNullOrEmpty(request.Phase))
            status.Phase = Enum.Parse<Models.Enums.StatusPhase>(request.Phase);

        await _globalStatusRepository.UpdateAsync(status);
        return true;
    }

    public async Task<bool> DeleteStatusAsync(int id)
    {
        return await _globalStatusRepository.DeleteAsync(id);
    }

    private static GlobalStatusResponse MapToResponse(Models.Domain.GlobalStatus status)
    {
        return new GlobalStatusResponse
        {
            Id = status.Id,
            StatusCode = status.StatusCode,
            DisplayName = status.DisplayName,
            Description = status.Description,
            Phase = status.Phase.ToString()
        };
    }
}