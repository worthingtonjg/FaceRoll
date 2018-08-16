namespace DataModels
{
    public class Attendee
    {
        public int AttendeeId { get; set; }

        public string AttendeeName { get; set; }

        public int MeetingId { get; set; }

        public override string ToString()
        {
            return AttendeeName;
        }
    }
}