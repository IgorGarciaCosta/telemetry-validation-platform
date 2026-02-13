using Api.Controllers;
using Api.Dtos;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Moq; // A biblioteca de Dublês
using Xunit;

namespace Api.Tests;

using DtoCreateEventRequest = Api.Dtos.CreateEventRequest;
public class EventsControllerTests
{
    // O Mock é o objeto que vai "fingir" ser o Service
    private readonly Mock<ITelemetryService> _mockService;

    // O Controller é quem vamos testar de verdade
    private readonly EventsController _controller;

    public EventsControllerTests()
    {
        // 1. Criamos o Dublê
        _mockService = new Mock<ITelemetryService>();

        // 2. Injetamos o Dublê no Controller (em vez do Service real)
        _controller = new EventsController(_mockService.Object);
    }

    [Fact]
    public async Task Create_ComRequestValido_DeveRetornar201Created()
    {
        // --- ARRANGE (Preparação) ---

        // Dados de entrada
        var request = new DtoCreateEventRequest
        {
            Type = "teste_mock",
            Payload = "{}"
        };

        // O que o Service deve devolver (simulado)
        var eventoRetornado = new TelemetryEvent
        {
            Id = Guid.NewGuid(),
            Type = request.Type,
            Payload = request.Payload,
            Timestamp = DateTimeOffset.UtcNow
        };

        // AQUI ESTÁ A MÁGICA DO MOQ:
        // Ensinamos o dublê: "Quando chamarem CreateEventAsync com qualquer string, retorne 'eventoRetornado'"
        _mockService
            .Setup(s => s.CreateEventAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(eventoRetornado);

        // --- ACT (Ação) ---
        var resultado = await _controller.Create(request);

        // --- ASSERT (Verificação) ---

        // 1. O resultado deve ser do tipo CreatedAtAction
        var actionResult = Assert.IsType<CreatedAtActionResult>(resultado.Result);

        // 2. O Status Code deve ser 201
        Assert.Equal(201, actionResult.StatusCode);

        // 3. O valor retornado deve ser um EventResponse (nosso DTO)
        var responseDto = Assert.IsType<EventResponse>(actionResult.Value);
        Assert.Equal(request.Type, responseDto.Type);
    }

    [Fact]
    public async Task GetAll_DeveRetornarListaDeEventos()
    {
        // --- ARRANGE ---
        // Vamos simular que o banco tem 2 eventos
        var listaFalsa = new List<TelemetryEvent>
        {
            new TelemetryEvent { Id = Guid.NewGuid(), Type = "A", Payload = "{}" },
            new TelemetryEvent { Id = Guid.NewGuid(), Type = "B", Payload = "{}" }
        };

        // Ensinamos o dublê a devolver essa lista quando chamarem GetAllEventsAsync
        _mockService
            .Setup(s => s.GetAllEventsAsync(null, null, null))
            .ReturnsAsync(listaFalsa);

        // --- ACT ---
        var resultado = await _controller.GetAll(null, null, null);

        // --- ASSERT ---
        var okResult = Assert.IsType<OkObjectResult>(resultado.Result); // Esperamos 200 OK
        var retorno = Assert.IsAssignableFrom<IEnumerable<EventResponse>>(okResult.Value);

        Assert.Equal(2, retorno.Count()); // Verifica se vieram 2 itens
    }
}
