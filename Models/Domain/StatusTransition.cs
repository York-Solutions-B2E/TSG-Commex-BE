namespace TSG_Commex_BE.Models.Domain;

public class StatusTransition
{
    public int Id { get; set; }
    public required string TypeCode { get; set; }
    public string? FromStatusCode { get; set; }  // null = initial state
    public required string ToStatusCode { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public CommunicationType CommunicationType { get; set; } = null!;
    public GlobalStatus? FromStatus { get; set; }
    public GlobalStatus ToStatus { get; set; } = null!;
    public List<CommunicationStatusHistory> StatusHistories { get; set; } = new();
} 