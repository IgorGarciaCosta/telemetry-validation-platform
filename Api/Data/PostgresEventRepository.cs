using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class PostgresEventRepository : IEventRepository
{
    private readonly AppDbContext _context;

    public PostgresEventRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TelemetryEvent> CreateAsync(TelemetryEvent @event)
    {
        _context.Events.Add(@event);
        await _context.SaveChangesAsync();
        return @event;
    }

    public async Task<TelemetryEvent?> GetByIdAsync(Guid id)
    {
        return await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<(IEnumerable<TelemetryEvent> Events, int TotalCount)> GetAllAsync(
        string? type,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page = 1,
        int pageSize = 10)
    {
        var query = _context.Events.AsQueryable();

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(e => e.Type == type);

        if (from.HasValue)
            query = query.Where(e => e.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(e => e.Timestamp <= to.Value);

        int totalCount = await query.CountAsync();

        var events = await query
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (events, totalCount);
    }
}