public class CommunicationStatusHistory
{
    public int Id { get; set; }
    public int CommunicationId { get; set; }
    public string StatusCode { get; set; }
    public DateTime OccurredUtc { get; set; }
    public string? Notes { get; set; }
    public string? EventSource { get; set; } // rabbitMQ, Manual

    public Communication Communication { get; set; }



    // public string Description { get; set; }
    // public bool IsActive { get; set; }

    // public List<CommunicationTypeStatus> ValidStatuses { get; set; }

}