using System.Threading.Tasks;
using Api.Data;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]

public class EventsController : ControllerBase
{
    private readonly ITelemetryService _service;
    public EventsController(ITelemetryService service)
    {
        _service = service;
    }



    [HttpPost]//POST api/events
    public async Task<ActionResult<TelemetryEvent>> Create([FromBody] CreateEventRequest request)
    {
        //1.basic valudation
        if (string.IsNullOrWhiteSpace(request.Type))
        {
            return BadRequest("Type is required.");
        }

        try
        {
            // O Controller apenas repassa o pedido para o Service
            // Tenta criar o evento através do serviço
            var novoEvento = await _service.CreateEventAsync(request.Type, request.Payload);

            //4. returns the created event with 201 Created status
            return CreatedAtAction(nameof(GetById), new { id = novoEvento.Id }, novoEvento);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });//400 error to user
        }


    }

    //GET api/events
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TelemetryEvent>>> GetAll([FromQuery] string? type, [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to)
    {
        var eventos = await _service.GetAllEventsAsync(type, from, to);
        return Ok(eventos);//200
    }

    //GET api/events/{id}/
    [HttpGet("{id:guid}")]//guid is a global unique identifier  
    public async Task<ActionResult<TelemetryEvent>> GetById(Guid id)
    {
        // 7. Busca no banco pelo ID (SELECT * FROM Events WHERE Id = ...)
        var evento = await _service.GetEventByIdAsync(id);
        if (evento == null)
        {
            return NotFound();//404
        }
        return Ok(evento);//200
    }
}