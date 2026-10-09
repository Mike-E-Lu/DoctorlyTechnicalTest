using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scheduler.Infrastructure.Persistence;
using Scheduler.Infrastructure.Presistence;

namespace Scheduler.Api.Tests;

/// <summary>
/// Boots the real API in memory against a throwaway LocalDB database.
/// Each factory instance (one per test class) gets its own database,
/// so test classes never see each other's data. The database is
/// created by the app's own Migrate() on startup and dropped on dispose.
/// </summary>
public class SchedulerApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString =
        $"Server=(localdb)\\MSSQLLocalDB;Database=Scheduler_Tests_{Guid.NewGuid():N};" +
        "Trusted_Connection=True;TrustServerCertificate=True";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // UseSetting is applied before Program.cs reads configuration,
        // so AddInfrastructure picks up the test connection string.
        builder.UseSetting("ConnectionStrings:Scheduler", _connectionString);
        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            using var scope = Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<SchedulerDBContext>().Database.EnsureDeleted();
        }
        base.Dispose(disposing);
    }
}