namespace Api.Models;


//data class
public record CreateEventRequest
(
    string Type,
    string? Payload
);