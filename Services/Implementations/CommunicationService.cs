using TSG_Commex_Shared.DTOs;
using TSG_Commex_Shared.DTOs.Request;
using TSG_Commex_BE.Models.Domain;
using Microsoft.EntityFrameworkCore;
using TSG_Commex_BE.Repositories.Interfaces;
using TSG_Commex_BE.Services.Interfaces;
using System.Linq;

namespace TSG_Commex_BE.Services.Implementations;

public class CommunicationService : ICommunicationService
{
    private readonly ICommunicationRepository _communicationRepository;
    private readonly ICommunicationTypeStatusRepository _communicationTypeStatusRepository;
    private readonly ICommunicationTypeRepository _communicationTypeRepository;
    private readonly IGlobalStatusRepository _globalStatusRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly ILogger<CommunicationService> _logger;

    public CommunicationService(
        ICommunicationRepository communicationRepository,
        ICommunicationTypeStatusRepository communicationTypeStatusRepository,
        ICommunicationTypeRepository communicationTypeRepository,
        IGlobalStatusRepository globalStatusRepository,
        IMemberRepository memberRepository,
        ILogger<CommunicationService> logger)
    {
        _communicationRepository = communicationRepository;
        _communicationTypeStatusRepository = communicationTypeStatusRepository;
        _communicationTypeRepository = communicationTypeRepository;
        _globalStatusRepository = globalStatusRepository;
        _memberRepository = memberRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<CommunicationResponse>> GetAllCommunicationsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all communications");
            var communications = await _communicationRepository.GetAllAsync();

            return communications.Select(c => MapToResponse(c)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all communications");
            throw;
        }
    }

    public async Task<CommunicationResponse?> GetCommunicationByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Fetching communication with ID: {Id}", id);
            var communication = await _communicationRepository.GetByIdAsync(id);

            return communication == null ? null : MapToResponse(communication);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching communication with ID: {Id}", id);
            throw;
        }
    }

    public async Task<CommunicationResponse> CreateCommunicationAsync(CreateCommunicationRequest request)
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ArgumentException("Title is required", nameof(request));

            // Validate member exists
            var member = await _memberRepository.GetByIdAsync(request.MemberId) ?? throw new InvalidOperationException($"Member with ID {request.MemberId} not found");

            // Validate communication type exists
            var communicationType = await _communicationTypeRepository.GetByIdAsync(request.CommunicationTypeId) ?? throw new InvalidOperationException($"Communication type with ID {request.CommunicationTypeId} not found");

            // Use the provided InitialStatusId or default to ReadyForRelease
            int statusId;
            if (request.InitialStatusId.HasValue)
            {
                var status = await _globalStatusRepository.GetByIdAsync(request.InitialStatusId.Value) ?? throw new InvalidOperationException($"Invalid status ID: {request.InitialStatusId}");
                statusId = request.InitialStatusId.Value;
            }
            else
            {
                var defaultStatus = await _globalStatusRepository.GetByStatusCodeAsync("ReadyForRelease") ?? throw new InvalidOperationException("Default status 'ReadyForRelease' not found");
                statusId = defaultStatus.Id;
            }

            // Map DTO → Domain Model
            var communication = new Communication
            {
                Title = request.Title,
                CommunicationTypeId = request.CommunicationTypeId,
                MemberId = request.MemberId,
                SourceFileUrl = request.SourceFileUrl,
                CurrentStatusId = statusId,
                CreatedByUserId = request.CreatedByUserId,
                IsActive = true
            };

            // Call repository
            var createdCommunication = await _communicationRepository.CreateAsync(communication);

            // Set navigation properties that we already have
            createdCommunication.Member = member;
            createdCommunication.CurrentStatus = await _globalStatusRepository.GetByIdAsync(statusId) ?? throw new InvalidOperationException($"Status with ID {statusId} not found");
            createdCommunication.CommunicationType = communicationType;

            // Map Domain Model → Response DTO
            return MapToResponse(createdCommunication);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating communication");
            throw;
        }
    }
    public async Task<bool> UpdateCommunicationAsync(int id, UpdateCommunicationRequest request)
    {
        try
        {
            var communication = await _communicationRepository.GetByIdAsync(id);
            if (communication == null)
                return false;

            // Update fields if provided
            if (!string.IsNullOrWhiteSpace(request.Title))
                communication.Title = request.Title;

            if (request.MemberId.HasValue && request.MemberId.Value > 0)
                communication.MemberId = request.MemberId.Value;

            if (!string.IsNullOrWhiteSpace(request.SourceFileUrl))
                communication.SourceFileUrl = request.SourceFileUrl;

            if (request.CurrentStatusId.HasValue)
                communication.CurrentStatusId = request.CurrentStatusId.Value;

            if (request.UpdatedByUserId.HasValue)
                communication.LastUpdatedByUserId = request.UpdatedByUserId.Value;

            // Timestamp is set in the repository
            await _communicationRepository.UpdateAsync(communication);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating communication with ID: {Id}", id);
            throw;
        }
    }

    public async Task<bool> DeleteCommunicationAsync(int id)
    {
        try
        {
            var communication = await _communicationRepository.GetByIdAsync(id);
            if (communication == null)
                return false;

            await _communicationRepository.DeleteAsync(id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting communication with ID: {Id}", id);
            throw;
        }
    }

    public async Task<bool> UpdateStatusAsync(int id, int newStatusId, int? userId = null)
    {
        try
        {
            var communication = await _communicationRepository.GetByIdAsync(id);
            if (communication == null)
                return false;

            // Validate status exists
            var status = await _globalStatusRepository.GetByIdAsync(newStatusId);
            if (status == null)
                return false;

            communication.CurrentStatusId = newStatusId;
            // Timestamp is set in the repository
            if (userId.HasValue)
                communication.LastUpdatedByUserId = userId.Value;

            await _communicationRepository.UpdateAsync(communication);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing status for communication ID: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<CommunicationResponse>> GetByStatusAsync(int statusId)
    {
        try
        {
            var communications = await _communicationRepository.GetByStatusAsync(statusId);
            return communications.Select(c => MapToResponse(c)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching communications by status ID: {StatusId}", statusId);
            throw;
        }
    }

    public async Task<IEnumerable<CommunicationResponse>> GetByTypeAsync(int typeId)
    {
        try
        {
            var communications = await _communicationRepository.GetByTypeId(typeId);
            return communications.Select(c => MapToResponse(c)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching communications by type ID: {TypeId}", typeId);
            throw;
        }
    }

    public async Task<IEnumerable<CommunicationResponse>> GetByMemberIdAsync(int memberId)
    {
        try
        {
            _logger.LogInformation("Fetching communications for member ID: {MemberId}", memberId);
            var communications = await _communicationRepository.GetByMemberIdAsync(memberId);
            return communications.Select(c => MapToResponse(c)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching communications for member ID: {MemberId}", memberId);
            throw;
        }
    }

    public async Task<IEnumerable<TSG_Commex_Shared.DTOs.CommunicationStatusHistory>?> GetCommunicationStatusHistoryAsync(int communicationId)
    {
        try
        {
            _logger.LogInformation("Fetching status history for communication {Id}", communicationId);

            // First check if communication exists
            var communication = await _communicationRepository.GetByIdAsync(communicationId);
            if (communication == null)
            {
                _logger.LogWarning("Communication {Id} not found", communicationId);
                return null;
            }

            // Get status history from repository
            var history = await _communicationRepository.GetStatusHistoryAsync(communicationId);

            // Map to DTOs
            var historyDtos = history.Select(h => new TSG_Commex_Shared.DTOs.CommunicationStatusHistory
            {
                Id = h.Id,
                CommunicationId = h.CommunicationId,
                StatusCode = h.GlobalStatus?.StatusCode ?? "Unknown",
                OccurredUtc = h.OccurredUtc,
                Notes = h.Notes,
                EventSource = h.EventSource,
                UpdatedByUserName = h.UpdatedByUser?.Email ?? "System"
            }).ToList();

            _logger.LogInformation("Found {Count} status history records for communication {Id}",
                historyDtos.Count(), communicationId);

            return historyDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching status history for communication {Id}", communicationId);
            throw;
        }
    }

    private static CommunicationResponse MapToResponse(Communication communication)
    {
        return new CommunicationResponse
        {
            Id = communication.Id,
            CommunicationTypeId = communication.CommunicationTypeId,
            TypeCode = communication.CommunicationType?.TypeCode ?? string.Empty,
            CurrentStatusId = communication.CurrentStatusId,
            CurrentStatus = communication.CurrentStatus?.StatusCode ?? string.Empty,
            MemberId = communication.MemberId,
            MemberName = communication.Member != null ?
                $"{communication.Member.FirstName} {communication.Member.LastName}" : string.Empty,
            Subject = communication.Title,
            Message = communication.SourceFileUrl,
            RecipientInfo = communication.Member?.Email ?? "Unknown",
            CreatedUtc = communication.CreatedUtc,
            LastUpdatedUtc = communication.LastUpdatedUtc,
            CreatedByUserName = communication.CreatedByUser?.Email ?? "System"
        };
    }


}