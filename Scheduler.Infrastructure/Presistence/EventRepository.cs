using Microsoft.EntityFrameworkCore;
using Scheduler.Application;
using Scheduler.Domain.Event;
using Scheduler.Infrastructure.Presistence;

namespace Scheduler.Infrastructure.Persistence;

public class EventRepository(SchedulerDBContext db) : IEventRepository
{
    public void Add(Event calendarEvent)
    {
        db.Events.Add(calendarEvent);
    }

    public Task<Event?> GetAsync(Guid id, CancellationToken ct = default)
        => db.Events.SingleOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<Event>> ListAsync(EventFilter filter, CancellationToken ct = default)
    {
        var query = db.Events.AsNoTracking();

        if (!filter.IncludeCancelled)
            query = query.Where(e => e.Status != EventStatus.Cancelled);
        if (filter.From is { } from)
            query = query.Where(e => e.EndTime > from);
        if (filter.To is { } to)
            query = query.Where(e => e.StartTime < to);
        if (!string.IsNullOrWhiteSpace(filter.AttendeeEmail))
        {
            var email = filter.AttendeeEmail.Trim().ToLowerInvariant();
            query = query.Where(e => e.Attendees.Any(a => a.Email == email));
        }
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search.Trim()}%";
            query = query.Where(e => EF.Functions.Like(e.Title, pattern) || EF.Functions.Like(e.Description, pattern));
        }

        return await query
            .OrderBy(e => e.StartTime)
            .Skip(filter.Skip)
            .Take(filter.Take)
            .ToListAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyException("The event was modified by someone else. Reload and retry.");
        }
    }
}
