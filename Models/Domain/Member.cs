namespace TSG_Commex_BE.Models.Domain;

public class Member
{
    public int Id { get; set; }
    public required string MemberId { get; set; }  // External member ID like "12345"
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public List<Communication> Communications { get; set; } = new();
    
    // Computed property for display
    public string FullName => $"{FirstName} {LastName}";
}