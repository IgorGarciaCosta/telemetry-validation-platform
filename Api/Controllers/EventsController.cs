using System.Threading.Tasks;
using Api.Data;
using Api.Dtos;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]

[Authorize] // Ninguém entra sem token válido
public class EventsController : ControllerBase
{
    private readonly ITelemetryService _service;
    public EventsController(ITelemetryService service)
    {
        _service = service;
    }



    [HttpPost]//POST api/events
    [Authorize(Roles = "Admin")] // Só Admin cria eventos
    public async Task<ActionResult<TelemetryEvent>> Create([FromBody] Api.Dtos.CreateEventRequest request)
    {
        //1.basic valudation
        if (string.IsNullOrWhiteSpace(request.Type))
        {
            return BadRequest("Type is required.");
        }


        // O Controller apenas repassa o pedido para o Service
        // Tenta criar o evento através do serviço
        var novoEvento = await _service.CreateEventAsync(request.Type, request.Payload);
        var response = new EventResponse
        {
            Id = novoEvento.Id,
            Type = novoEvento.Type,
            Timestamp = novoEvento.Timestamp,
            Payload = novoEvento.Payload ?? string.Empty
        };

        //4. returns the created event with 201 Created status
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);

    }

    //GET api/events
    [HttpGet]
    [Authorize(Roles = "Admin,Reader")] // Admin e Leitor podem ver
    public async Task<ActionResult<IEnumerable<TelemetryEvent>>> GetAll([FromQuery] string? type, [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var eventos = await _service.GetAllEventsAsync(type, from, to);
        var response = eventos.Select(e => new EventResponse
        {
            Id = e.Id,
            Type = e.Type,
            Timestamp = e.Timestamp,
            Payload = e.Payload ?? string.Empty
        });

        return Ok(response);//200
    }

    //GET api/events/{id}/
    [HttpGet("{id:guid}")]//guid is a global unique identifier  
    [Authorize(Roles = "Admin,Reader")]
    public async Task<ActionResult<TelemetryEvent>> GetById(Guid id)
    {
        // 7. Busca no banco pelo ID (SELECT * FROM Events WHERE Id = ...)
        var evento = await _service.GetEventByIdAsync(id);

        if (evento == null) return NotFound();//404

        var response = new EventResponse
        {
            Id = evento.Id,
            Type = evento.Type,
            Timestamp = evento.Timestamp,
            Payload = evento.Payload ?? string.Empty
        };

        return Ok(response);//200
    }
}