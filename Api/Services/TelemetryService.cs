using Api.Data;
using Api.Models;
using System.Text.Json;

namespace Api.Services;

public class TelemetryService : ITelemetryService
{
    private readonly IEventRepository _repository;

    // O Service precisa do Banco, então injeto o DbContext aqui
    public TelemetryService(IEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<TelemetryEvent> CreateEventAsync(string type, string? payload)
    {
        try
        {
            JsonDocument.Parse(payload ?? string.Empty);
        }
        catch (JsonException)
        {
            // Se o payload não for um JSON válido, lança uma exceção
            throw new ArgumentException("Payload must be a valid JSON string.", nameof(payload));
        }

        var novoEvento = new TelemetryEvent
        {
            Id = Guid.NewGuid(),
            Type = type,
            Timestamp = DateTimeOffset.UtcNow,
            Payload = payload
        };

        return await _repository.CreateAsync(novoEvento);

    }

    public async Task<IEnumerable<TelemetryEvent>> GetAllEventsAsync(string? type, DateTimeOffset? from, DateTimeOffset? to, int page = 1, int pageSize = 10)
    {
        var (events, _) = await _repository.GetAllAsync(type, from, to, page, pageSize);

        return events;
    }

    public async Task<TelemetryEvent?> GetEventByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

}
