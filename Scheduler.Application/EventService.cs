using Scheduler.Domain.Common;
using Scheduler.Domain.Event;

namespace Scheduler.Application;

/// <summary>Application service: orchestrates the use cases, the domain does the business rules.</summary>
public class EventService(IEventRepository repository)
{
    public const int MaxPageSize = 100;

    public async Task<EventDto> GetAsync(Guid id, CancellationToken ct = default)
        => EventDto.From(await LoadAsync(id, ct));

    public async Task<IReadOnlyList<EventDto>> ListAsync(EventFilter filter, CancellationToken ct = default)
    {
        filter = filter with { Skip = Math.Max(0, filter.Skip), Take = Math.Clamp(filter.Take, 1, MaxPageSize) };
        var events = await repository.ListAsync(filter, ct);
        return events.Select(EventDto.From).ToList();
    }

    private async Task<Event> LoadAsync(Guid id, CancellationToken ct)
           => await repository.GetAsync(id, ct) ?? throw new NotFoundException($"Event {id} was not found.");
}
