using TSG_Commex_Shared.DTOs;

namespace TSG_Commex_BE.Services.Interfaces;

public interface IMemberService
{
    Task<IEnumerable<MemberDto>> GetAllMembersAsync();
    Task<MemberDto?> GetMemberByIdAsync(int id);
    Task<MemberDto> CreateMemberAsync(CreateMemberDto dto);
    Task<MemberDto?> UpdateMemberAsync(int id, UpdateMemberDto dto);
    Task<bool> DeleteMemberAsync(int id);
    Task<IEnumerable<MemberDto>> GetActiveMembersAsync();
}