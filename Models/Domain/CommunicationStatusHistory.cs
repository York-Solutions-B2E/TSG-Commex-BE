using System.ComponentModel.DataAnnotations.Schema;

namespace TSG_Commex_BE.Models.Domain;

public class CommunicationStatusHistory
{
    public int Id { get; set; }
    public int CommunicationId { get; set; }
    public int GlobalStatusId { get; set; } // Foreign key to GlobalStatus
    public DateTime OccurredUtc { get; set; }
    public string? Notes { get; set; }
    public string? EventSource { get; set; } // RabbitMQ, Manual, Simulator
    public int? UpdatedByUserId { get; set; } // Who made the change

    // Navigation properties
    public Communication Communication { get; set; } = null!;
    public GlobalStatus GlobalStatus { get; set; } = null!;
    public User? UpdatedByUser { get; set; }
}