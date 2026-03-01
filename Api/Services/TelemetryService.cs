using Api.Data;
using Api.Models;
using System.Text.Json;

namespace Api.Services;

public class TelemetryService : ITelemetryService
{
    private readonly IEventRepository _repository;
    private readonly IMessageQueueService _queueService;

    // O Service precisa do Banco, então injeto o DbContext 
    //injeta o serviço de fila para enviar mensagens quando um evento for criado
    public TelemetryService(IEventRepository repository, IMessageQueueService queueService)
    {
        _repository = repository;
        _queueService = queueService;
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

        // 1. Salva no Banco (Dynamo ou Postgres)
        var createdEvent = await _repository.CreateAsync(novoEvento);

        // 2. Envia para a fila SQS
        var eventJson = JsonSerializer.Serialize(createdEvent);
        await _queueService.SendMessageAsync(eventJson);

        return createdEvent;

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
