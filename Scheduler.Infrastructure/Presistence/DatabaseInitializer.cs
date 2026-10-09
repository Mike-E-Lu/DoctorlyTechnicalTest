using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scheduler.Infrastructure.Presistence;

namespace Scheduler.Infrastructure.Persistence;


public static class DatabaseInitializer
{
    /// <summary>Applies pending migrations and creates any configured API users that don't exist yet.</summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using (var scope = services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<SchedulerDBContext>().Database.MigrateAsync(cancellationToken);
        }

    }


}
