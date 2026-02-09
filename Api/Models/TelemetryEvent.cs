namespace Api.Models;

public class TelemetryEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();//global ID

    public string Type { get; set; } = string.Empty;//event type   

    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;//event timestamp

    public string? Payload { get; set; }//Any extra data (JSON, Text, etc)
}