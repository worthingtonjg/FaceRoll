using DataAccess;
using DataModels;
using FaceRoll.Common;
using FaceRoll.Model;
using FaceRoll.Pages;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Windows.Input;

namespace FaceRoll.ViewModels
{
    public class FaceNotFoundViewModel : ViewModelBase
    {
        public bool CalledFromViewAttendance { get; set; }

        [XamlProperty]
        public string PersonName { get; set; }

        [XamlProperty]
        public ICommand SubmitFaceCommand
        {
            get
            {
                return new DelegateCommand(async (o) => 
                {
                    if (PersonName == null) return;

                    // Add the persons name to the roll
                    await Repository.AddAttendee(App.ActiveMeeting.MeetingId, new Attendee { AttendeeName = PersonName, MeetingId = App.ActiveMeeting.MeetingId });

                    // If not called from view attendance then register face with cognitive service, and then get ready to take another person
                    if (!CalledFromViewAttendance)
                    {
                        var clipboardHelper = new ClipboardHelper();
                        var image = await clipboardHelper.ImageFromClipboard();

                        var faceHelper = new FaceHelper(
                            SettingsHelper.ReadSettings(SettingsHelper.FaceApiSubscriptionKey),
                            SettingsHelper.ReadSettings(SettingsHelper.FaceApiRoot));

                        var personGroupId = SettingsHelper.ReadSettings(SettingsHelper.FaceApiPersonGroup);

                        var result = await faceHelper.AddPerson(personGroupId, PersonName);
                        await faceHelper.AddImageToPerson(personGroupId, result.PersonId, image);
                        await faceHelper.TrainGroup(personGroupId);

                        NavigationHelper.Navigate(typeof(FaceFoundPage), new Identification(new Person() { Name = PersonName }, 1, null, null));
                    }
                    else
                    {
                        NavigationHelper.Navigate(typeof(ViewAttendancePage), App.ActiveMeeting);
                    }
                });
            }
        }

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
    }
}
