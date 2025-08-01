using System.Text.Json.Serialization;

namespace TSG_Commex_BE.DTOs.Events;

public class CommunicationStatusChangedEvent
{
    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = "CommunicationStatusChanged";

    [JsonPropertyName("communicationId")]
    public string CommunicationId { get; set; } = string.Empty;

    [JsonPropertyName("newStatus")]
    public string NewStatus { get; set; } = string.Empty;

    [JsonPropertyName("timestampUtc")]
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public Dictionary<string, string> Metadata { get; set; } = new();

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}