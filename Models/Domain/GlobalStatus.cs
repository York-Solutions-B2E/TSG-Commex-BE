using TSG_Commex_BE.Models.Enums;

namespace TSG_Commex_BE.Models.Domain;

public class GlobalStatus
{
    public required string StatusCode { get; set; }
    public required string DisplayName { get; set; }
    public required string Description { get; set; }
    public StatusPhase Phase { get; set; }
    public bool IsActive { get; set; } = true;


    // Navigation properties
    public List<Communication> Communications { get; set; } = new();
    public List<CommunicationStatusHistory> StatusHistories { get; set; } = new();

}