namespace TSG_Commex_BE.Models.Domain;

public class User
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Role { get; set; } = "User"; // User, Admin
    public bool IsActive { get; set; } = true;
    public DateTime CreatedUtc { get; set; }
    public DateTime LastLoginUtc { get; set; }

    // Navigation properties
    public List<Communication> CreatedCommunications { get; set; } = new();
    public List<Communication> LastUpdatedCommunications { get; set; } = new();
    public List<CommunicationStatusHistory> StatusHistoryEntries { get; set; } = new();

    // Computed property for display
    public string FullName => $"{FirstName} {LastName}";
}