using Scheduler.Application;
using Scheduler.Domain.Common;
using Scheduler.Domain.Event;
using Microsoft.Extensions.Logging;

namespace Scheduler.Infrastructure.Notifications;

public record EmailMessage(string To, string Subject, string Body);

/// <summary>Transport for emails. Swap the implementation for SMTP/SendGrid/etc. in production.</summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}

public class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        logger.LogInformation("Email to {To}: {Subject}\n{Body}", message.To, message.Subject, message.Body);
        return Task.CompletedTask;
    }
}

/// <summary>Turns domain events into emails to the affected attendees.</summary>
public class EmailNotificationService(IEmailSender sender, ILogger<EmailNotificationService> logger) : INotificationService
{
    public async Task PublishAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken ct = default)
    {
        foreach (var message in domainEvents.SelectMany(ToMessages))
        {
            try
            {
                await sender.SendAsync(message, ct);
            }
            catch (Exception ex)
            {
                // The change is already saved; a failed notification must not fail the request.
                logger.LogError(ex, "Failed to send notification to {To}", message.To);
            }
        }
    }

    private static IEnumerable<EmailMessage> ToMessages(IDomainEvent domainEvent) => domainEvent switch
    {
        EventScheduled(var e) => e.Attendees.Select(a => Mail(a.Email, $"Invitation: {e.Title}", e)),
        AttendeeInvited(var e, var a) => [Mail(a.Email, $"Invitation: {e.Title}", e)],
        EventUpdated(var e) => e.Attendees.Select(a => Mail(a.Email, $"Updated: {e.Title}", e)),
        EventCancelled(var e) => e.Attendees.Select(a => Mail(a.Email, $"Cancelled: {e.Title}", e)),
        AttendeeRemoved(var e, _, var email) => [Mail(email, $"Removed from: {e.Title}", e)],
        AttendeeResponded(var e, var a) => e.Attendees.Where(x => x.Id != a.Id).Select(x => new EmailMessage(x.Email,
            $"{a.Name} {(a.IsAttending == true ? "accepted" : "declined")}: {e.Title}", Body(e))),
        _ => []
    };

    private static EmailMessage Mail(string to, string subject, Event e) => new(to, subject, Body(e));

    private static string Body(Event e) =>
        $"{e.Title}\n{e.StartTime:u} - {e.EndTime:u}\n\n{e.Description}";
}
