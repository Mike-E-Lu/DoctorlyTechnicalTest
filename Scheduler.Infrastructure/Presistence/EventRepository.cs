using Microsoft.EntityFrameworkCore;
using Scheduler.Application;
using Scheduler.Domain.Event;
using Scheduler.Infrastructure.Presistence;

namespace Scheduler.Infrastructure.Persistence;

public class EventRepository(SchedulerDBContext db) : IEventRepository
{
    public void Add(Event calendarEvent)
    {
        throw new NotImplementedException();
    }

    public Task<Event?> GetAsync(Guid id, CancellationToken ct = default)
        => db.Events.SingleOrDefaultAsync(e => e.Id == id, ct);

    public Task<IReadOnlyList<Event>> ListAsync(EventFilter filter, CancellationToken ct = default)
    {
        throw new NotImplementedException();
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
