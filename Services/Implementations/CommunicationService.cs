using TSG_Commex_BE.DTOs.Response;
using TSG_Commex_BE.DTOs.Request;
using TSG_Commex_BE.Models.Domain;
using TSG_Commex_BE.Repositories.Interfaces;
using TSG_Commex_BE.Services.Interfaces;

namespace TSG_Commex_BE.Services.Implementations;

public class CommunicationService : ICommunicationService
{
    private readonly ICommunicationRepository _communicationRepository;
    private readonly ICommunicationTypeStatusRepository _communicationTypeStatusRepository;

    public CommunicationService(
        ICommunicationRepository communicationRepository,
        ICommunicationTypeStatusRepository communicationTypeStatusRepository)
    {
        _communicationRepository = communicationRepository;
        _communicationTypeStatusRepository = communicationTypeStatusRepository;
    }

    // TODO: Implement all interface methods

    // public async Task<CommunicationResponse> CreateCommunicationAsync(CreateCommunicationRequest request)
    // {
    //     // Validation
    //     if (string.IsNullOrWhiteSpace(request.Title))
    //         throw new ArgumentException("Title is required", nameof(request));

    //     if (string.IsNullOrWhiteSpace(request.TypeCode))
    //         throw new ArgumentException("TypeCode is required", nameof(request));

    //     // Map DTO → Domain Model
    //     var communication = new Communication
    //     {
    //         Title = request.Title,
    //         TypeCode = request.TypeCode,
    //         MemberInfo = request.MemberInfo,
    //         SourceFileUrl = request.SourceFileUrl,
    //         CurrentStatus = "ReadyForRelease", // Default initial status
    //         IsActive = true,
    //         CreatedUtc = DateTime.UtcNow,
    //         LastUpdatedUtc = DateTime.UtcNow,
    //         CreatedByUserId = int.TryParse(request.CreatedByUserId, out var userId) ? userId : null
    //     };

    //     // Call repository
    //     var createdCommunication = await _communicationRepository.CreateAsync(communication);

    //     // Map Domain Model → Response DTO
    //     return MapToResponse(createdCommunication);
    // }
    // public async Task<CommunicationResponse> GetCommunicationResponseAsync(int id)
    // {
    //     _logger.LogInformation("Retrieving Communication ID: {id}", id);
    //     var communication = await _communicationRepository.GetByIdAsync(id);
    //     // return communication != null ? CommunicationResponse.From
    // }

    // private static CommunicationResponse MapToResponse(Communication communication)
    // {
    //     return new CommunicationResponse
    //     {
    //         Id = communication.Id,
    //         TypeCode = communication.TypeCode,
    //         Title = communication.Title,
    //         MemberInfo = communication.MemberInfo,
    //         CurrentStatus = communication.CurrentStatus,
    //         SourceFileUrl = communication.SourceFileUrl,
    //         CreatedUtc = communication.CreatedUtc,
    //         LastUpdatedUtc = communication.LastUpdatedUtc,
    //         CreatedByUserName = communication.CreatedByUser?.Name ?? "System"
    //     };
    // }


}