using System.ComponentModel.DataAnnotations.Schema;

namespace TSG_Commex_BE.Models.Domain;

public class Communication
{
    public Communication()
    {
        StatusHistory = new List<CommunicationStatusHistory>();
    }
    public int Id { get; set; }
    public required string Title { get; set; } // "John Doe - EOB Q3 2024"
    [ForeignKey("Type")]
    public required string TypeCode { get; set; }  // "EOB", "EOP", "ID_CARD"
    public required string CurrentStatus { get; set; } // "Printed", "InTransit"
    public DateTime CreatedUtc { get; set; }
    public DateTime LastUpdatedUtc { get; set; }
    public string? SourceFileUrl { get; set; }  // Mock URL to source document
    public string? MemberInfo { get; set; }     // "Member ID: 12345"


    //Navigation properties
    public CommunicationType Type { get; set; }

    [InverseProperty("Communication")] // clarifies back reference
    public List<CommunicationStatusHistory> StatusHistory { get; set; }



}
