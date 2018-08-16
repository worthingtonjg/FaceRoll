using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DataModels;
using System.Threading.Tasks;

namespace DataAccess
{
    public class Repository
    {
        public static async Task AddMeeting(Meeting meeting, FaceRollContext context = null)
        {
            await Task.Run(() => 
            {
                using (context == null ? context = new FaceRollContext() : null)
                {
                    context.Meetings.Add(meeting);

                    context.SaveChanges();
                }
            });
        }

        public static async Task<bool> DeleteMeeting(int meetingId, FaceRollContext context = null)
        {
            bool result = false;

            await Task.Run(() => 
            {
                using (context == null ? context = new FaceRollContext() : null)
                {
                    var match = context.Meetings.FirstOrDefault(m => m.MeetingId == meetingId);

                    if (match != null)
                    {
                        // Remove attendees
                        var attendees = context.Attendees.Where(a => a.MeetingId == meetingId).ToList();
                        context.Attendees.RemoveRange(attendees);
                        
                        // Remove meeting
                        context.Meetings.Remove(match);
                        result = true;
                    }

                    context.SaveChanges();
                }
            });

            return result;
        }

        public static async Task AddAttendee(int meetingId, Attendee attendee, FaceRollContext context = null)
        {
            await Task.Run(() => 
            {
                using (context == null ? context = new FaceRollContext() : null)
                {
                    if (!context.Attendees.Any(a => a.MeetingId == meetingId && a.AttendeeName.Trim().ToLower() == attendee.AttendeeName.Trim().ToLower()))
                    {
                        context.Attendees.Add(attendee);
                        context.SaveChanges();
                    }
                }
            });
        }

        public static async Task<bool> AlreadyAttending(int meetingId, string name, FaceRollContext context = null)
        {
            bool result = false;

            await Task.Run(() =>
            {
            using (context == null ? context = new FaceRollContext() : null)
            {
                result = context.Attendees.Any(
                    a => a.MeetingId == meetingId && 
                    a.AttendeeName.Trim().ToLower() == name.Trim().ToLower());
                }
            });

            return result;
        }

        public static async Task<bool> DeleteAttendee(int attendeeId, FaceRollContext context = null)
        {
            bool result = false;

            await Task.Run(() => 
            {
                using (context == null ? context = new FaceRollContext() : null)
                {
                    var match = context.Meetings.FirstOrDefault(m => m.MeetingId == attendeeId);

                    if (match != null)
                    {
                        context.Meetings.Remove(match);
                        result = true;
                    }

                    context.SaveChanges();
                }
            });

            return result;
        }

        public static async Task<T> FindByKey<T>(int key, FaceRollContext context = null) where T : class
        {
            T result = null;

            await Task.Run(() =>
            {
                using (context == null ? context = new FaceRollContext() : null)
                {
                    result = context.Set<T>().Find(key);

                    if (result != null)
                    {
                        object entity = result as object;
                    }
                }
            });

            return result;
        }

        public static async Task<List<T>> FindBy<T>(System.Linq.Expressions.Expression<Func<T, bool>> predicate, FaceRollContext context = null) where T : class
        {
            var result = new List<T>();

            await Task.Run(() => 
            { 
                using (context == null ? context = new FaceRollContext() : null)
                {
                    result = context.Set<T>().Where(predicate).ToList();

                    foreach (var item in result)
                    {
                        object entity = item as object;
                    }
                }
            });

            return result;
        }

        public static async Task<List<T>> GetAll<T>(FaceRollContext context = null) where T : class
        {
            var result = new List<T>();

            await Task.Run(() =>
            {
                using (context == null ? context = new FaceRollContext() : null)
                {
                    result = context.Set<T>().AsQueryable().ToList();

                    foreach (var item in result)
                    {
                        object entity = item as object;
                    }
                }
            });

            return result;
        }
    }
}
