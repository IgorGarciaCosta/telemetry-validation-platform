using Api.Data;
using Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Xunit;

namespace Api.Tests;

public class TelemetryServiceTests
{
    // Método auxiliar para criar um banco "zerado" para cada teste
    private AppDbContext GetDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Nome único para não misturar testes
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateEvent_ComJsonValido_DeveSalvarNoBanco()
    {
        // 1. ARRANGE (Preparação)
        var context = GetDatabaseContext();
        var service = new TelemetryService(context);

        string tipo = "teste_unitario";
        string payload = "{\"chave\": \"valor\"}"; // JSON Válido

        // 2. ACT (Ação)
        var resultado = await service.CreateEventAsync(tipo, payload);

        // 3. ASSERT (Verificação)

        // Verifica se o objeto retornado não é nulo
        Assert.NotNull(resultado);

        // Verifica se o ID foi gerado
        Assert.NotEqual(Guid.Empty, resultado.Id);

        // Verifica se salvou no banco (Count deve ser 1)
        var totalNoBanco = await context.Events.CountAsync();
        Assert.Equal(1, totalNoBanco);
    }

    [Fact]
    public async Task CreateEvent_ComJsonInvalido_DeveLancarErro()
    {
        // 1. ARRANGE
        var context = GetDatabaseContext();
        var service = new TelemetryService(context);

        string tipo = "teste_erro";
        string payloadRuim = "Isso não é um json";

        // 2. ACT & ASSERT
        // Aqui dizemos: "Espero que, ao chamar esse método, aconteça uma ArgumentException"
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await service.CreateEventAsync(tipo, payloadRuim);
        });
    }

}
