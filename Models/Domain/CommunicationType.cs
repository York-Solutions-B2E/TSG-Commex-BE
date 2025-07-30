public class CommunicationType
{
    public string TypeCode { get; set; }  // Primary key: "EOB", "EOP", "ID_CARD"
    public string DisplayName { get; set; }  // "Explanation of Benefits"
    public string Description { get; set; }
    public bool IsActive { get; set; }

    public List<CommunicationTypeStatus> ValidStatuses { get; set; }
    public List<Communication> Communications { get; set; }
}