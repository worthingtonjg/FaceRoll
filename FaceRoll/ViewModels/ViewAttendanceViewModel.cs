using DataAccess;
using DataModels;
using FaceRoll.Common;
using FaceRoll.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FaceRoll.ViewModels
{
    public class ViewAttendanceViewModel : ViewModelBase
    {
        private Meeting _meeting;

        [XamlProperty]
        public DateTime MeetingDate { get; set; }

        [XamlProperty]
        public string MeetingName { get; set; }
        
        [XamlProperty]
        public List<Attendee> Attendees { get; set; }

        [XamlProperty]
        public ICommand BackCommand
        {
            get
            {
                return new DelegateCommand((o) => 
                {
                    NavigationHelper.Navigate(typeof(ManageMeetingsPage));
                });
            }
        }

        [XamlProperty]
        public ICommand DrawWinnerCommand
        {
            get
            {
                return new DelegateCommand((o) => 
                {
                    NavigationHelper.Navigate(typeof(DrawWinnerPage));
                });
            }
        }

        [XamlProperty]
        public ICommand AddAttendeeCommand
        {
            get
            {
                return new DelegateCommand((o) =>
                {
                    NavigationHelper.Navigate(typeof(FaceNotFoundPage), true);
                });
            }
        }

        public void SetMeetingDetails(Meeting meeting)
        {
            _meeting = meeting;

            SetValue(() => MeetingDate, meeting.MeetingDate);
            SetValue(() => MeetingName, meeting.MeetingName);
        }

        public async Task RefreshAttendees()
        {
            var attendees = await Repository.FindBy<Attendee>(a => a.MeetingId == _meeting.MeetingId);

            attendees = attendees.OrderBy(a => a.AttendeeName).ToList();

            SetValue(() => Attendees, attendees);
        }
    }
}