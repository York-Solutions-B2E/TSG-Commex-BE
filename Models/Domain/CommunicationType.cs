namespace TSG_Commex_BE.Models.Domain;

public class CommunicationType
{
    public required string TypeCode { get; set; }  // Primary key: "EOB", "EOP", "ID_CARD"
    public required string DisplayName { get; set; }  // "Explanation of Benefits"
    public required string Description { get; set; }
    public bool IsActive { get; set; }

    public List<CommunicationTypeStatus> ValidStatuses { get; set; }
    public List<Communication> Communications { get; set; }
}