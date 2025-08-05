namespace TSG_Commex_BE.Models.Domain;

public class CommunicationType
{
    public int Id { get; set; }
    public required string TypeCode { get; set; }  // Unique: "EOB", "EOP", "ID_CARD"
    public required string DisplayName { get; set; }  // "Explanation of Benefits"
    public required string Description { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public List<Communication> Communications { get; set; } = new();
    // Note: StatusTransitions are now handled separately - no direct navigation needed
}