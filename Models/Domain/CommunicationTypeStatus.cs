namespace TSG_Commex_BE.Models.Domain;

public class CommunicationTypeStatus
{
    public int Id { get; set; }
    public int CommunicationTypeId { get; set; }
    public int GlobalStatusId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }

    // Navigation properties
    public CommunicationType CommunicationType { get; set; } = null!;
    public GlobalStatus GlobalStatus { get; set; } = null!;
}