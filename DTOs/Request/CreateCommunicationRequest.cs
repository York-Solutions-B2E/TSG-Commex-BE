namespace TSG_Commex_BE.DTOs.Request;

public class CreateCommunicationRequest
{
    public string TypeCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? MemberInfo { get; set; }
    public string? SourceFileUrl { get; set; }
}