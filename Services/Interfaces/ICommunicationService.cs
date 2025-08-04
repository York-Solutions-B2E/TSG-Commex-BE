using TSG_Commex_BE.DTOs.Request;
using TSG_Commex_BE.DTOs.Response;
using TSG_Commex_BE.Models.Domain;

namespace TSG_Commex_BE.Services.Interfaces;

public interface ICommunicationService
{
    Task<IEnumerable<CommunicationResponse>> GetAllCommunicationsAsync();
    Task<CommunicationResponse?> GetCommunicationByIdAsync(int id);
    Task<CommunicationResponse> CreateCommunicationAsync(CreateCommunicationRequest request);
    Task<bool> UpdateCommunicationAsync(int id, UpdateCommunicationRequest request);
    Task<bool> DeleteCommunicationAsync(int id);
    Task<bool> ChangeStatusAsync(int id, string newStatus, string userId);
    Task<IEnumerable<CommunicationResponse>> GetByStatusAsync(string status);
    Task<IEnumerable<CommunicationResponse>> GetByTypeAsync(string typeCode);
}