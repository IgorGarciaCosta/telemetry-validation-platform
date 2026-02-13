namespace Api.Dtos;

public class EventResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string Payload { get; set; } = string.Empty;

    public string SizeInfo => Payload.Length > 50 ? "Large" : "Small";
}