using Microsoft.AspNetCore.Mvc;
using Scheduler.Api.Contracts.Requests;
using Scheduler.Application;
using Scheduler.Domain.Event;

namespace Scheduler.Api.Controllers
{
    public class EventController : BaseApiController
    {
        //TODO: Implement the EventController methods for handling event-related API requests
        EventService service;

        public EventController(EventService service)
        {
            this.service = service;
        }

        //Update
        //Remove
        [HttpGet]
        [ProducesResponseType<IReadOnlyList<EventDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<EventDto>>> List ([FromQuery] ListEventQuery query, CancellationToken cancellationToken)
        {
            var events = await service.ListAsync(
            new EventFilter(query.From, query.To, query.AttendeeEmail, query.Search, query.IncludeCancelled, query.Skip, query.Take), cancellationToken);
            return Ok(events);
        }

        /// <summary>Create a new event. Attendees are notified.</summary>
        [HttpPost(Name = "CreateEvent")]
        [ProducesResponseType<EventDto>(StatusCodes.Status201Created)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<EventDto>> Create(
            [FromBody] CreateEventRequest request, CancellationToken ct)
        {
            var created = await service.CreateAsync(request, ct);
            return CreatedAtRoute("GetEvent", new { id = created.Id }, created);
        }

        /// <summary>Get a single event by id.</summary>
        [HttpGet("{id:guid}", Name = "GetEvent")]
        [ProducesResponseType<EventDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EventDto>> Get(Guid id, CancellationToken ct)
        {
            var ev = await service.GetAsync(id, ct);
            return ev is null ? NotFound() : Ok(ev);
        }
    }
}
