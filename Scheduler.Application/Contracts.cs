using System.ComponentModel.DataAnnotations;
using Scheduler.Domain.Event;

namespace Scheduler.Application;

public record AttendeeInput(

    [Required, MaxLength(Attendee.NameMaxLength)] string Name,
    [Required, EmailAddress, MaxLength(Attendee.EmailMaxLength)] string Email);

public record CreateEventRequest(
   [Required, MaxLength(200)] string Title,
    [MaxLength(2000)] string? Description,
    [Required] DateTime Start,
    [Required] DateTime End,
    [MaxLength(50)] List<AttendeeInput>? Attendees);

public record AttendeeDto(Guid Id, string Name, string Email, bool? IsAttending);

public record EventDto(
    Guid Id,
    string Title,
    string Description,
    DateTime StartTime,
    DateTime EndTime,
    EventStatus Status,
    int Version,
    IReadOnlyList<AttendeeDto> Attendees)
{
    public static EventDto From(Event e) => new(
        e.Id, e.Title, e.Description, e.StartTime, e.EndTime, e.Status, e.Version,
        e.Attendees.Select(a => new AttendeeDto(a.Id, a.Name, a.Email, a.IsAttending)).ToList());
}
