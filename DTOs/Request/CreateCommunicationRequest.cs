namespace TSG_Commex_BE.DTOs.Request;

public class CreateCommunicationRequest
{
    // TODO: Add properties for creating a new communication
    public string TypeCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? MemberInfo { get; set; }
    public string? SourceFileUrl { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;

}