using System.Text.Json.Serialization;

namespace TSG_Commex_BE.DTOs.Events;

public class CommunicationCreatedEvent
{
    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = "CommunicationCreated";

    [JsonPropertyName("communicationId")]
    public string CommunicationId { get; set; } = string.Empty;

    [JsonPropertyName("typeCode")]
    public string TypeCode { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("timestampUtc")]
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("source")]
    public string Source { get; set; } = "System";

    [JsonPropertyName("metadata")]
    public Dictionary<string, string> Metadata { get; set; } = new();
}