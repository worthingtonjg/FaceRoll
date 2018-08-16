using System;
using System.Collections.Generic;
using System.Text;

namespace DataModels
{
    public class Meeting
    {
        public int MeetingId { get; set; }

        public DateTime MeetingDate { get; set; }

        public string MeetingName { get; set; }

        public List<Attendee> Attendees { get; set; }
    }
}
