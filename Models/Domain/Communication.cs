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
    public int CommunicationTypeId { get; set; }  // Foreign key to CommunicationType
    public int CurrentStatusId { get; set; } // Foreign key to GlobalStatus
    public DateTime CreatedUtc { get; set; }
    public DateTime LastUpdatedUtc { get; set; }
    public string? SourceFileUrl { get; set; }  // Mock URL to source document
    public int MemberId { get; set; }           // Required - every communication must belong to a member
    public bool IsActive { get; set; } = true; // Soft delete support

    // User tracking
    public int? CreatedByUserId { get; set; }
    public int? LastUpdatedByUserId { get; set; }

    //Navigation properties
    public CommunicationType CommunicationType { get; set; } = null!;
    public GlobalStatus CurrentStatus { get; set; } = null!;
    public Member Member { get; set; } = null!;
    public User? CreatedByUser { get; set; }
    public User? LastUpdatedByUser { get; set; }

    [InverseProperty("Communication")] // clarifies back reference
    public List<CommunicationStatusHistory> StatusHistory { get; set; }



}
