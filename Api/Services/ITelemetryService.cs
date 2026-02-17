using Api.Models;

namespace Api.Services;

public interface ITelemetryService
{
    Task<TelemetryEvent> CreateEventAsync(string type, string? payload);
    Task<IEnumerable<TelemetryEvent>> GetAllEventsAsync(string? type, DateTimeOffset? from, DateTimeOffset? to, int page = 1, int pageSize = 10);
    Task<TelemetryEvent?> GetEventByIdAsync(Guid id);
}