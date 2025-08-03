namespace TSG_Commex_BE.DTOs.Response;

public class CommunicationResponse
{
    // TODO: Add properties for communication data sent to frontend
    public int Id { get; set; }
    public string TypeCode { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public string RecipientInfo { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public DateTime LastUpdatedUtc { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
}