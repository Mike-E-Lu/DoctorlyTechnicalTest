using Microsoft.EntityFrameworkCore;
using Scheduler.Domain.Event;
using System;
using System.Collections.Generic;
using System.Text;

namespace Scheduler.Infrastructure.Presistence
{
    public class SchedulerDBContext(DbContextOptions<SchedulerDBContext> options) : DbContext(options)
    {
        public DbSet<Event> Events => Set<Event>();

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchedulerDBContext).Assembly);
    }
}
