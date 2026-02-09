using System.Threading.Tasks;
using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]//exope to .NET that this is an API
[Route("api/[controller]")]//defines the base adderss http://localhost:5020/api/events

public class EventsController : ControllerBase
{

    private readonly AppDbContext _context;// 1. Em vez da lista estática, declaramos o Banco de Dados

    public EventsController(AppDbContext context)
    {
        //O .NET entrega o banco pronto pra gente no construtor
        _context = context;// 2. Injetamos o contexto do banco de dados no construtor do controlador
    }



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

        _context.Events.Add(novoEvento);//3. add the new event to the database context
        _context.SaveChanges();//4. save the changes to the database

        //4. returns the created event with 201 Created status
        return CreatedAtAction(nameof(GetById), new { id = novoEvento.Id }, novoEvento);
    }

    //GET api/events
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TelemetryEvent>>> GetAll([FromQuery] string? type, [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to)
    {
        var query = _context.Events.AsQueryable();//start with all events

        if (!string.IsNullOrWhiteSpace(type))//filter if the user sent a type 
        {
            query = query.Where(e => e.Type == type);// O EF traduz isso pra SQL: WHERE Type = '...'
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

        // 6. Vai no banco buscar os dados (ToListAsync)
        var eventos = await query.ToListAsync(); //execute the query and get the results as a list

        //return filtered list
        return Ok(eventos);
    }
    //GET api/events/{id}/
    [HttpGet("{id:guid}")]//guid is a global unique identifier  
    public async Task<ActionResult<TelemetryEvent>> GetById(Guid id)
    {
        // 7. Busca no banco pelo ID (SELECT * FROM Events WHERE Id = ...)
        var evento = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);//find the event with the given id
        if (evento == null)
        {
            return NotFound();//404
        }
        return Ok(evento);//200
    }
}