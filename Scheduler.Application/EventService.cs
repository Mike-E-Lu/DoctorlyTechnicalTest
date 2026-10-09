using Scheduler.Domain.Common;
using Scheduler.Domain.Event;

namespace Scheduler.Application;

/// <summary>Application service: orchestrates the use cases, the domain does the business rules.</summary>
public class EventService(IEventRepository repository, INotificationService notifications)
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

    public async Task<EventDto> CreateAsync(CreateEventRequest request, CancellationToken ct = default)
    {
        var e = Event.Schedule(request.Title, request.Description ?? "", request.Start, request.End,
            ToTuples(request.Attendees));
        repository.Add(e);
        await SaveAndNotifyAsync(e, ct);
        return EventDto.From(e);
    }

    private async Task SaveAndNotifyAsync(AggregateRoot aggregate, CancellationToken ct)
    {
        await repository.SaveChangesAsync(ct);
        var domainEvents = aggregate.DomainEvents.ToList();
        aggregate.ClearDomainEvents();
        await notifications.PublishAsync(domainEvents, ct);
    }

    private static IEnumerable<(string, string)> ToTuples(IEnumerable<AttendeeInput>? attendees)
        => (attendees ?? []).Select(a => (a.Name, a.Email));

    private async Task<Event> LoadAsync(Guid id, CancellationToken ct)
           => await repository.GetAsync(id, ct) ?? throw new NotFoundException($"Event {id} was not found.");
}
