using TSG_Commex_Shared.DTOs;
using TSG_Commex_BE.Models.Domain;
using TSG_Commex_BE.Repositories.Interfaces;
using TSG_Commex_BE.Services.Interfaces;

namespace TSG_Commex_BE.Services.Implementations;

public class MemberService : IMemberService
{
    private readonly IMemberRepository _memberRepository;
    private readonly ILogger<MemberService> _logger;

    public MemberService(IMemberRepository memberRepository, ILogger<MemberService> logger)
    {
        _memberRepository = memberRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<MemberDto>> GetAllMembersAsync()
    {
        var members = await _memberRepository.GetAllAsync();
        return members.Select(MapToDto);
    }

    public async Task<MemberDto?> GetMemberByIdAsync(int id)
    {
        var member = await _memberRepository.GetByIdAsync(id);
        return member != null ? MapToDto(member) : null;
    }

    public async Task<MemberDto> CreateMemberAsync(CreateMemberDto dto)
    {
        // Check if member ID already exists
        var existing = await _memberRepository.GetByMemberIdAsync(dto.MemberId);
        if (existing != null)
        {
            throw new InvalidOperationException($"Member with ID {dto.MemberId} already exists");
        }

        // Check if email already exists
        existing = await _memberRepository.GetByEmailAsync(dto.Email);
        if (existing != null)
        {
            throw new InvalidOperationException($"Member with email {dto.Email} already exists");
        }

        var member = new Member
        {
            MemberId = dto.MemberId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            ZipCode = dto.ZipCode,
            DateOfBirth = dto.DateOfBirth,
            EnrollmentDate = dto.EnrollmentDate,
            IsActive = dto.IsActive
        };

        var created = await _memberRepository.CreateAsync(member);
        _logger.LogInformation($"Created new member: {created.MemberId} - {created.FullName}");
        
        return MapToDto(created);
    }

    public async Task<MemberDto?> UpdateMemberAsync(int id, UpdateMemberDto dto)
    {
        var member = await _memberRepository.GetByIdAsync(id);
        if (member == null)
        {
            return null;
        }

        // Check if new member ID is already taken by another member
        if (member.MemberId != dto.MemberId)
        {
            var existing = await _memberRepository.GetByMemberIdAsync(dto.MemberId);
            if (existing != null && existing.Id != id)
            {
                throw new InvalidOperationException($"Member ID {dto.MemberId} is already in use");
            }
        }

        // Check if new email is already taken by another member
        if (member.Email.ToLower() != dto.Email.ToLower())
        {
            var existing = await _memberRepository.GetByEmailAsync(dto.Email);
            if (existing != null && existing.Id != id)
            {
                throw new InvalidOperationException($"Email {dto.Email} is already in use");
            }
        }

        member.MemberId = dto.MemberId;
        member.FirstName = dto.FirstName;
        member.LastName = dto.LastName;
        member.Email = dto.Email;
        member.PhoneNumber = dto.PhoneNumber;
        member.Address = dto.Address;
        member.City = dto.City;
        member.State = dto.State;
        member.ZipCode = dto.ZipCode;
        member.DateOfBirth = dto.DateOfBirth;
        member.EnrollmentDate = dto.EnrollmentDate;
        member.IsActive = dto.IsActive;

        var updated = await _memberRepository.UpdateAsync(member);
        _logger.LogInformation($"Updated member: {updated.MemberId} - {updated.FullName}");
        
        return MapToDto(updated);
    }

    public async Task<bool> DeleteMemberAsync(int id)
    {
        var result = await _memberRepository.DeleteAsync(id);
        if (result)
        {
            _logger.LogInformation($"Deleted member with ID: {id}");
        }
        return result;
    }

    public async Task<IEnumerable<MemberDto>> GetActiveMembersAsync()
    {
        var members = await _memberRepository.GetActiveAsync();
        return members.Select(MapToDto);
    }

    private static MemberDto MapToDto(Member member)
    {
        return new MemberDto
        {
            Id = member.Id,
            MemberId = member.MemberId,
            FirstName = member.FirstName,
            LastName = member.LastName,
            FullName = member.FullName,
            Email = member.Email,
            PhoneNumber = member.PhoneNumber,
            Address = member.Address,
            City = member.City,
            State = member.State,
            ZipCode = member.ZipCode,
            DateOfBirth = member.DateOfBirth,
            EnrollmentDate = member.EnrollmentDate,
            IsActive = member.IsActive,
            CreatedUtc = member.CreatedUtc,
            LastUpdatedUtc = member.LastUpdatedUtc,
            CommunicationCount = member.Communications?.Count ?? 0
        };
    }
}