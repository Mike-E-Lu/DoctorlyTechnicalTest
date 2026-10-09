using System.ComponentModel.DataAnnotations;

namespace Scheduler.Api.Contracts.Requests
{
    public class ListEventQuery
    {
        public DateTimeOffset? From { get; init; }
        public DateTimeOffset? To { get; init; }

        [EmailAddress, MaxLength(254)]
        public string? AttendeeEmail { get; init; }

        [MaxLength(100)]
        public string? Search { get; init; }

        public bool IncludeCancelled { get; init; } = false;

        [Range(0, int.MaxValue)]
        public int Skip { get; init; } = 0;

        [Range(1, 100)]
        public int Take { get; init; } = 50;
    }
}
