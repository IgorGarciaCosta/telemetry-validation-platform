using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using Xunit;

namespace Api.IntegrationTests;

// IClassFixture garante que a API suba uma vez só para todos os testes desta classe
public class EventsIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EventsIntegrationTests(CustomWebApplicationFactory factory)
    {
        // O _client é como se fosse o Postman aberto
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task FluxoCompleto_CriarEBuscarEvento_DeveFuncionar()
    {
        // 1. ARRANGE (Preparar o JSON)
        var novoEvento = new CreateEventRequest
        {
            Type = "integracao_teste",
            Payload = "{\"teste\": \"funciona\"}"
        };

        // 2. ACT (Enviar POST para /api/events)
        var responseCreate = await _client.PostAsJsonAsync("/api/events", novoEvento);

        // 3. ASSERT (Verificar se criou)
        responseCreate.EnsureSuccessStatusCode(); // Garante que deu 200-299
        Assert.Equal(HttpStatusCode.Created, responseCreate.StatusCode);

        // Ler o objeto que voltou (para pegar o ID)
        var eventoCriado = await responseCreate.Content.ReadFromJsonAsync<EventResponse>();
        Assert.NotNull(eventoCriado);
        Assert.NotEqual(Guid.Empty, eventoCriado.Id);

        // --- PARTE 2: Tentar buscar esse evento pelo ID (GET) ---
        var responseGet = await _client.GetAsync($"/api/events/{eventoCriado.Id}");

        Assert.Equal(HttpStatusCode.OK, responseGet.StatusCode);

        var eventoBuscado = await responseGet.Content.ReadFromJsonAsync<EventResponse>();
        Assert.NotNull(eventoBuscado);
        Assert.Equal(novoEvento.Type, eventoBuscado.Type);
        Assert.Equal("Small", eventoBuscado.SizeInfo); // Testando nossa lógica do DTO também
    }
}
