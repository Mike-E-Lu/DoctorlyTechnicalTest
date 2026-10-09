using Scheduler.Domain.Common;

namespace Scheduler.Application;

/// <summary>Notifies interested parties (e.g. attendees by email) about things that happened in the domain.</summary>
public interface INotificationService
{
    Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default);
}
