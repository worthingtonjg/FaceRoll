using DataAccess;
using DataModels;
using FaceRoll.Common;
using FaceRoll.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FaceRoll.ViewModels
{
    public class DrawWinnerViewModel : ViewModelBase
    {
        private List<Attendee> _winners = new List<Attendee>();

        [XamlProperty]
        public string WinnerName { get; set; }

        [XamlProperty]
        public ICommand BackCommand
        {
            get
            {
                return new DelegateCommand((o) =>
                {
                    NavigationHelper.Navigate(typeof(ViewAttendancePage));
                });
            }
        }

        [XamlProperty]
        public ICommand DrawWinnerCommand
        {
            get
            {
                return new DelegateCommand(async (o) =>
                {
                    await DrawWinner();
                });
            }
        }

        public async Task DrawWinner()
        {
            SetValue(() => WinnerName, string.Empty);

            var attendees = await Repository.FindBy<Attendee>(a => a.MeetingId == App.ActiveMeeting.MeetingId);

            // No one to pick then we are done
            if (attendees.Count == 0 || attendees.Count - _winners.Count == 0)
            {
                SetValue(() => WinnerName, "No more names to pick from");
                return;
            }

            Attendee winner = null;

            while (winner == null)
            {
                Random rand = new Random();
                int index = rand.Next(attendees.Count);

                winner = attendees[index];
                if (_winners.Any(w => w.AttendeeId == winner.AttendeeId))
                {
                    winner = null;
                }
            }

            _winners.Add(winner);
            SetValue(() => WinnerName, $"The winner is: {winner.AttendeeName}");
        }
    }
}
