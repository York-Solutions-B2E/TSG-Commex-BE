using TSG_Commex_Shared.DTOs;
using TSG_Commex_Shared.DTOs.Request;
using TSG_Commex_BE.Models.Domain;

namespace TSG_Commex_BE.Services.Interfaces;

public interface ICommunicationService
{
    Task<IEnumerable<CommunicationResponse>> GetAllCommunicationsAsync();
    Task<CommunicationResponse?> GetCommunicationByIdAsync(int id);
    Task<CommunicationResponse> CreateCommunicationAsync(CreateCommunicationRequest request);
    Task<bool> UpdateCommunicationAsync(int id, UpdateCommunicationRequest request);
    Task<bool> DeleteCommunicationAsync(int id);
    Task<bool> ChangeStatusAsync(int id, int newStatusId, int? userId = null);
    Task<IEnumerable<CommunicationResponse>> GetByStatusAsync(int statusId);
    Task<IEnumerable<CommunicationResponse>> GetByTypeAsync(int typeId);
}