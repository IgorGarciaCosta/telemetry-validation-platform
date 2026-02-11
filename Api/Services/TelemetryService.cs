using Api.Data;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Api.Services;

public class TelemetryService : ITelemetryService
{
    private readonly AppDbContext _context;

    // O Service precisa do Banco, então injeto o DbContext aqui
    public TelemetryService(AppDbContext context)
    {
        _context = context;
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

        _context.Events.Add(novoEvento);
        await _context.SaveChangesAsync();

        return novoEvento;
    }

    public async Task<IEnumerable<TelemetryEvent>> GetAllEventsAsync(string? type, DateTimeOffset? from, DateTimeOffset? to)
    {
        var query = _context.Events.AsQueryable();

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(e => e.Type == type);

        if (from.HasValue)
            query = query.Where(e => e.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(e => e.Timestamp <= to.Value);

        return await query.ToListAsync();
    }

    public async Task<TelemetryEvent?> GetEventByIdAsync(Guid id)
    {
        return await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
    }

}
