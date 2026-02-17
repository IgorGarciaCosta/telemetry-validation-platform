using Api.Models;

namespace Api.Data;

public interface IEventRepository
{
    Task<TelemetryEvent> CreateAsync(TelemetryEvent @event);
    Task<TelemetryEvent?> GetByIdAsync(Guid id);
    Task<(IEnumerable<TelemetryEvent> Events, int TotalCount)> GetAllAsync(
        string? type,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page = 1,
        int pageSize = 10);
}