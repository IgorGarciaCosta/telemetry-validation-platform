using Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]//exope to .NET that this is an API
[Route("api/[controller]")]//defines the base adderss http://localhost:5020/api/events

public class EventsController : ControllerBase
{
    //temporary database
    //statis means that this list is shared among all instances of the EventsController class
    private static readonly List<TelemetryEvent> _events = new();

    [HttpPost]//POST api/events
    public ActionResult<TelemetryEvent> Create([FromBody] CreateEventRequest request)
    {
        //1.basic valudation
        if (string.IsNullOrWhiteSpace(request.Type))
        {
            return BadRequest("Type is required.");
        }

        //2.creates the final object
        var novoEvento = new TelemetryEvent
        {
            Id = Guid.NewGuid(),
            Type = request.Type,
            Timestamp = DateTimeOffset.UtcNow,
            Playload = request.Playload,
        };

        //3. saves in the list (simulating a data base)
        _events.Add(novoEvento);

        //4. returns the created event with 201 Created status
        return CreatedAtAction(nameof(GetById), new { id = novoEvento.Id }, novoEvento);
    }

    //GET api/events
    [HttpGet]
    public ActionResult<IEnumerable<TelemetryEvent>> GetAll([FromQuery] string? type, [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to)
    {
        var query = _events.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(type))//filter if the user sent a type 
        {
            query = query.Where(e => e.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
        }

        //if sent start date
        if (from.HasValue)
        {
            query = query.Where(e => e.Timestamp >= from.Value);
        }

        //if sent end date
        if (to.HasValue)
        {
            query = query.Where(e => e.Timestamp <= to.Value);
        }

        //return filtered list
        return Ok(query.ToList());
    }
    //GET api/events/{id}/
    [HttpGet("{id:guid}")]//guid is a global unique identifier  
    public ActionResult<TelemetryEvent> GetById(Guid id)
    {
        var evento = _events.FirstOrDefault(e => e.Id == id);
        if (evento == null)
        {
            return NotFound();//404
        }
        return Ok(evento);//200
    }
}