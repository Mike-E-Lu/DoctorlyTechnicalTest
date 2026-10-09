using System.ComponentModel.DataAnnotations;
using Scheduler.Domain.Event;

namespace Scheduler.Application;


public record AttendeeDto(Guid Id, string Name, string Email, bool? IsAttending);

public record EventDto(
    Guid Id,
    string Title,
    string Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    EventStatus Status,
    int Version,
    IReadOnlyList<AttendeeDto> Attendees)
{
    public static EventDto From(Event e) => new(
        e.Id, e.Title, e.Description, e.StartTime, e.EndTime, e.Status, e.Version,
        e.Attendees.Select(a => new AttendeeDto(a.Id, a.Name, a.Email, a.IsAttending)).ToList());
}
