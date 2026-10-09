using System;
using System.Collections.Generic;
using System.Text;

namespace Scheduler.Domain.Event
{
    internal class Event
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public  string Description { get; set; }
        public List<Attendee> Attendees { get; set; }
        public  DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }   

        public static void Create()
        {
            throw new NotImplementedException();
        }


    }
}
