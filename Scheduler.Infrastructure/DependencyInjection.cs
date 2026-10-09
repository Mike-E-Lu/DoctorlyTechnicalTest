using Scheduler.Application;
using Scheduler.Domain.Event;
using Scheduler.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scheduler.Infrastructure.Presistence;
using Scheduler.Infrastructure.Notifications;

namespace Scheduler.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {

        services.AddDbContext<SchedulerDBContext>(o => o.UseSqlServer(connectionString));
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<INotificationService, EmailNotificationService>();
        services.AddSingleton<IEmailSender, LoggingEmailSender>();
        services.AddScoped<EventService>();
        return services;
    }
}
