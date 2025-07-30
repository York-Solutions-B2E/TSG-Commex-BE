public class Communication
{
    public int Id { get; set; }
    public string Title { get; set; } // "John Doe - EOB Q3 2024"
    public required string TypeCode { get; set; }  // "EOB", "EOP", "ID_CARD"
    public string CurrentStatus { get; set; } // "Printed", "InTransit"
    public DateTime CreatedUtc { get; set; }
    public DateTime LastUpdatedUtc { get; set; }
    public string? SourceFileUrl { get; set; }  // Mock URL to source document
    public string? MemberInfo { get; set; }     // "Member ID: 12345"

    public CommunicationType Type { get; set; }
    public List<CommunicationStatusHistory> StatusHistory { get; set; }



}