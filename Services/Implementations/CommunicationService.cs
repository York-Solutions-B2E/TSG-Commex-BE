using TSG_Commex_BE.DTOs.Response;
using TSG_Commex_BE.DTOs.Request;
using TSG_Commex_BE.Models.Domain;
using Microsoft.EntityFrameworkCore;
using TSG_Commex_BE.Repositories.Interfaces;
using TSG_Commex_BE.Services.Interfaces;

namespace TSG_Commex_BE.Services.Implementations;

public class CommunicationService : ICommunicationService
{
    private readonly ICommunicationRepository _communicationRepository;
    private readonly ICommunicationTypeStatusRepository _communicationTypeStatusRepository;
    private readonly ILogger<CommunicationService> _logger;

    public CommunicationService(
        ICommunicationRepository communicationRepository,
        ICommunicationTypeStatusRepository communicationTypeStatusRepository,
        ILogger<CommunicationService> logger)
    {
        _communicationRepository = communicationRepository;
        _communicationTypeStatusRepository = communicationTypeStatusRepository;
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
        // Validation
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required", nameof(request));

        if (string.IsNullOrWhiteSpace(request.TypeCode))
            throw new ArgumentException("TypeCode is required", nameof(request));

        // Map DTO → Domain Model
        var communication = new Communication
        {
            Title = request.Title,
            TypeCode = request.TypeCode,
            MemberInfo = request.MemberInfo,
            SourceFileUrl = request.SourceFileUrl,
            CurrentStatus = "ReadyForRelease", // Default initial status
            IsActive = true,
            CreatedUtc = DateTime.UtcNow,
            LastUpdatedUtc = DateTime.UtcNow
        };

        // Call repository
        var createdCommunication = await _communicationRepository.CreateAsync(communication);

        // Map Domain Model → Response DTO
        return MapToResponse(createdCommunication);
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
            
            if (!string.IsNullOrWhiteSpace(request.MemberInfo))
                communication.MemberInfo = request.MemberInfo;
                
            communication.LastUpdatedUtc = DateTime.UtcNow;

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

    public async Task<bool> ChangeStatusAsync(int id, string newStatus, string userId)
    {
        try
        {
            var communication = await _communicationRepository.GetByIdAsync(id);
            if (communication == null)
                return false;

            communication.CurrentStatus = newStatus;
            communication.LastUpdatedUtc = DateTime.UtcNow;
            
            await _communicationRepository.UpdateAsync(communication);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing status for communication ID: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<CommunicationResponse>> GetByStatusAsync(string status)
    {
        try
        {
            var communications = await _communicationRepository.GetByStatusAsync(status);
            return communications.Select(c => MapToResponse(c)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching communications by status: {Status}", status);
            throw;
        }
    }

    public async Task<IEnumerable<CommunicationResponse>> GetByTypeAsync(string typeCode)
    {
        try
        {
            var communications = await _communicationRepository.GetByTypeCode(typeCode);
            return communications.Select(c => MapToResponse(c)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching communications by type: {TypeCode}", typeCode);
            throw;
        }
    }

    private static CommunicationResponse MapToResponse(Communication communication)
    {
        return new CommunicationResponse
        {
            Id = communication.Id,
            TypeCode = communication.TypeCode,
            Subject = communication.Title, // Map Title to Subject
            Message = communication.MemberInfo, // Map MemberInfo to Message
            CurrentStatus = communication.CurrentStatus,
            RecipientInfo = communication.MemberInfo ?? "Unknown",
            CreatedUtc = communication.CreatedUtc,
            LastUpdatedUtc = communication.LastUpdatedUtc,
            CreatedByUserName = communication.CreatedByUser?.FullName ?? "System"
        };
    }


}