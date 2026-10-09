using Scheduler.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Scheduler.Domain.Event
{
    public class Event : AggregateRoot
    {
        public const int TitleMaxLength = 200;
        public const int DescriptionMaxLength = 4000;
        public const int MaxAttendees = 100;

        private readonly List<Attendee> _attendees = [];

        public Guid Id { get; set; }
        public string Title { get; set; }
        public  string Description { get; set; }
        public  DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public EventStatus Status { get; private set; }
        public IReadOnlyCollection<Attendee> Attendees => _attendees;

        public int Version { get; private set; }

        private Event() { } // for EF: materialisation only,

        public static void Create()
        {
            throw new NotImplementedException();
        }

        public static Event Schedule(string title, string description, DateTime start, DateTime end,
        IEnumerable<(string Name, string Email)> attendees)
        {
            var e = new Event { Id = Guid.NewGuid(), Status = EventStatus.Scheduled, Version = 1 };
            e.SetDetails(title, description, start, end);
            e.SyncAttendees(attendees, raiseEvents: false);
            e.Raise(new EventScheduled(e));
            return e;
        }

        private void SetDetails(string title, string description, DateTime start, DateTime end)
        {
            title = title?.Trim() ?? "";
            description = description?.Trim() ?? "";
            if (title.Length == 0) throw new DomainException("Title is required.");
            if (title.Length > TitleMaxLength) throw new DomainException($"Title must be at most {TitleMaxLength} characters.");
            if (description.Length > DescriptionMaxLength) throw new DomainException($"Description must be at most {DescriptionMaxLength} characters.");
            if (end <= start) throw new DomainException("End time must be after start time.");

            Title = title;
            Description = description;
            StartTime = start.ToUniversalTime();
            EndTime = end.ToUniversalTime();
        }

        /// <summary>
        /// Replaces the attendee list, matching by email so existing attendees keep their response.
        /// </summary>
        private void SyncAttendees(IEnumerable<(string Name, string Email)> attendees, bool raiseEvents)
        {
            var requested = attendees.Select(a => (a.Name, Email: Attendee.NormalizeEmail(a.Email))).ToList();
            if (requested.Count > MaxAttendees) throw new DomainException($"An event can have at most {MaxAttendees} attendees.");
            if (requested.Select(a => a.Email).Distinct().Count() != requested.Count)
                throw new DomainException("Attendee emails must be unique.");

            foreach (var removed in _attendees.Where(a => requested.All(r => r.Email != a.Email)).ToList())
            {
                _attendees.Remove(removed);
                if (raiseEvents) Raise(new AttendeeRemoved(this, removed.Name, removed.Email));
            }

            foreach (var (name, email) in requested)
            {
                var existing = _attendees.SingleOrDefault(a => a.Email == email);
                if (existing is not null)
                {
                    existing.Rename(name);
                    continue;
                }

                var attendee = new Attendee(name, email);
                _attendees.Add(attendee);
                if (raiseEvents) Raise(new AttendeeInvited(this, attendee));
            }
        }
    }
}
