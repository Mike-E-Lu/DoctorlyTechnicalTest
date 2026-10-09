using Scheduler.Domain.Common;

namespace Scheduler.Domain.Event;

public record EventScheduled(Event Event) : IDomainEvent;
public record EventUpdated(Event Event) : IDomainEvent;
public record EventCancelled(Event Event) : IDomainEvent;
public record AttendeeInvited(Event Event, Attendee Attendee) : IDomainEvent;
public record AttendeeRemoved(Event Event, string Name, string Email) : IDomainEvent;
public record AttendeeResponded(Event Event, Attendee Attendee) : IDomainEvent;
