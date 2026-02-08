using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Isso diz: "Crie uma tabela chamada 'Events' baseada no modelo TelemetryEvent"
    public DbSet<TelemetryEvent> Events { get; set; }
}