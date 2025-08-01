namespace TSG_Commex_BE.Models.Domain;

public class CommunicationTypeStatus
{
    public required string TypeCode { get; set; }
    public required string StatusCode { get; set; }
    public string? Description { get; set; }

    // Navigation properties
    public CommunicationType CommunicationType { get; set; } = null!;
    public GlobalStatus GlobalStatus { get; set; } = null!;
}