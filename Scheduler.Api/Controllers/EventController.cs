using Microsoft.AspNetCore.Mvc;
using Scheduler.Api.Contracts.Requests;
using Scheduler.Application;
using Scheduler.Domain.Event;

namespace Scheduler.Api.Controllers
{
    public class EventController : BaseApiController
    {
        //TODO: Implement the EventController methods for handling event-related API requests

        //Create
        //Update
        //Remove
        [HttpGet]
        [ProducesResponseType<IReadOnlyList<EventDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<EventDto>>> List ([FromServices] EventService service, [FromQuery] ListEventQuery query, CancellationToken cancellationToken)
        {
            var events = await service.ListAsync(
            new EventFilter(query.From, query.To, query.AttendeeEmail, query.Search, query.IncludeCancelled, query.Skip, query.Take), cancellationToken);
            return Ok(events);
        }
    }
}
