using DataAccess;
using DataModels;
using FaceRoll.Common;
using FaceRoll.Model;
using FaceRoll.Pages;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace FaceRoll.ViewModels
{
    public class FaceFoundViewModel : ViewModelBase
    {
        private DispatcherTimer _timer = new DispatcherTimer();

        [XamlProperty]
        public string PersonName { get; set; }

        public async Task AddPersonAndContinue(Identification identification)
        {
            Debug.WriteLine(identification.Confidence);
            SetValue(() => PersonName, identification.Person.Name);

            await Repository.AddAttendee(App.ActiveMeeting.MeetingId, new Attendee { AttendeeName = identification.Person.Name, MeetingId = App.ActiveMeeting.MeetingId });

            _timer.Tick += Timer_Tick;
            _timer.Interval = new TimeSpan(0, 0, int.Parse(SettingsHelper.ReadSettings(SettingsHelper.MatchFoundPause)));
            _timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            _timer.Stop();
            NavigationHelper.Navigate(typeof(TakePhotoPage));
        }
    }
}
