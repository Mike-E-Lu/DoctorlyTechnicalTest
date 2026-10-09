namespace Scheduler.Domain.Event;

public interface IEventRepository
{
    Task<Event?> GetAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Event>> ListAsync(EventFilter filter, CancellationToken ct = default);

    void Add(Event calendarEvent);

    Task SaveChangesAsync(CancellationToken ct = default);
}

/// <param name="From">Only events that end after this time.</param>
/// <param name="To">Only events that start before this time.</param>
/// <param name="AttendeeEmail">Only events this email is invited to.</param>
/// <param name="Search">Free text matched against title and description.</param>
public record EventFilter(
    DateTimeOffset? From = null,
    DateTimeOffset? To = null,
    string? AttendeeEmail = null,
    string? Search = null,
    bool IncludeCancelled = false,
    int Skip = 0,
    int Take = 50);
