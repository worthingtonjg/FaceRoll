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
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace FaceRoll.ViewModels
{
    public class ManageMeetingsViewModel : ViewModelBase
    {
        public ManageMeetingsViewModel()
        {
            SetValue(() => LocalStoragePath, Windows.Storage.ApplicationData.Current.LocalFolder.Path);
            SetValue(() => NewMeetingDate, DateTime.Today);
        }

        [XamlProperty]
        public List<Meeting> Meetings { get; set; }

        [XamlProperty]
        public DateTime NewMeetingDate { get; set; }

        [XamlProperty]
        public string NewMeetingName { get; set; }

        [XamlProperty]
        public string LocalStoragePath { get; set; }

        public ICommand AddMeetingCommand
        {
            get
            {
                return new DelegateCommand(async (o) => 
                {
                    

                    await Repository.AddMeeting(new Meeting { MeetingName = NewMeetingName, MeetingDate = NewMeetingDate.Date });

                    await RefreshMeetings();

                    SetValue(() => NewMeetingDate, DateTime.Today);
                    SetValue(() => NewMeetingName, string.Empty);
                });
            }
        }

        [XamlProperty]
        public ICommand StartMeetingCommand
        {
            get
            {
                return new DelegateCommand((meeting) => 
                {
                    NavigationHelper.Navigate(typeof(TakePhotoPage), meeting);
                });
            }
        }

        [XamlProperty]
        public ICommand ViewMeetingCommand
        {
            get
            {
                return new DelegateCommand((meeting) => 
                {
                    NavigationHelper.Navigate(typeof(ViewAttendancePage), meeting);
                });
            }
        }

        [XamlProperty]
        public ICommand DeleteMeetingCommand
        {
            get
            {
                return new DelegateCommand(async (o) => 
                {
                    var dialog = new ContentDialog
                    {
                        Title = "Delete meeting?",
                        Content = "Delete meeting?",
                        PrimaryButtonText = "Delete",
                        CloseButtonText = "Cancel"
                    };

                    var result = await dialog.ShowAsync();

                    if(result == ContentDialogResult.Primary)
                    {
                        var meeting = o as Meeting;
                        await Repository.DeleteMeeting(meeting.MeetingId);
                        await RefreshMeetings();
                    }
                });
            }
        }


        public async Task RefreshMeetings()
        {
            var meetings = await Repository.GetAll<Meeting>();

            SetValue(() => Meetings, meetings);
        }
    
    }
}
