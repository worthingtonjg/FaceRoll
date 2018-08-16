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
    public class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel()
        {
            SetValue(() => FaceApiSubscriptionKey, SettingsHelper.ReadSettings(SettingsHelper.FaceApiSubscriptionKey));
            SetValue(() => FaceApiRoot, SettingsHelper.ReadSettings(SettingsHelper.FaceApiRoot));
            SetValue(() => FaceApiPersonGroup, SettingsHelper.ReadSettings(SettingsHelper.FaceApiPersonGroup));
            SetValue(() => MatchFoundPause, 3);
            SetValue(() => TimeBetweenPhotos, 3);
            SetValue(() => FaceBoxSize, 50);

            string matchFoundPause = SettingsHelper.ReadSettings(SettingsHelper.MatchFoundPause);
            int matchFoundPauseInt = 0;
            if (int.TryParse(matchFoundPause, out matchFoundPauseInt))
            {
                SetValue(() => MatchFoundPause, matchFoundPauseInt);
            }

            string timeBetweenPhotos = SettingsHelper.ReadSettings(SettingsHelper.TimeBetweenPhotos);
            int timeBetweenPhotosInt = 0;
            if (int.TryParse(timeBetweenPhotos, out timeBetweenPhotosInt))
            {
                SetValue(() => TimeBetweenPhotos, timeBetweenPhotosInt);
            }

            string faceBoxSize = SettingsHelper.ReadSettings(SettingsHelper.FaceBoxSize);
            int faceBoxSizeInt = 0;
            if (int.TryParse(faceBoxSize, out faceBoxSizeInt))
            {
                SetValue(() => FaceBoxSize, faceBoxSizeInt);
            }
        }

        [XamlProperty]
        public string FaceApiSubscriptionKey { get; set; }

        [XamlProperty]
        public string FaceApiRoot { get; set; }

        [XamlProperty]
        public string FaceApiPersonGroup { get; set; }

        [XamlProperty]
        public int TimeBetweenPhotos { get; set; }

        [XamlProperty]
        public int MatchFoundPause { get; set; }

        [XamlProperty]
        public int FaceBoxSize { get; set; }

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
        public ICommand SaveSettingsCommand
        {
            get
            {
                return new DelegateCommand((o) =>
                {
                    SettingsHelper.SaveSetting(SettingsHelper.FaceApiSubscriptionKey, FaceApiSubscriptionKey);
                    SettingsHelper.SaveSetting(SettingsHelper.FaceApiRoot, FaceApiRoot);
                    SettingsHelper.SaveSetting(SettingsHelper.FaceApiPersonGroup, FaceApiPersonGroup);

                    if (MatchFoundPause < 1) SetValue(() => MatchFoundPause, 1);
                    if (TimeBetweenPhotos < 1) SetValue(() => TimeBetweenPhotos, 1);
                    if (FaceBoxSize < 25) SetValue(() => FaceBoxSize, 25);

                    SettingsHelper.SaveSetting(SettingsHelper.MatchFoundPause, MatchFoundPause.ToString());
                    SettingsHelper.SaveSetting(SettingsHelper.TimeBetweenPhotos, TimeBetweenPhotos.ToString());
                    SettingsHelper.SaveSetting(SettingsHelper.FaceBoxSize, FaceBoxSize.ToString());

                    NavigationHelper.Navigate(typeof(ManageMeetingsPage));
                });
            }
        }
    }
}
