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


    }
}
