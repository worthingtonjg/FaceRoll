using FaceRoll.Common;
using FaceRoll.Pages;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FaceRoll
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            NavigationHelper.NavigationFrame = NavigationFrame;

            bool needSettings = false;
            if (SettingsHelper.ReadSettings(SettingsHelper.FaceApiSubscriptionKey) == string.Empty) needSettings = true;
            if (SettingsHelper.ReadSettings(SettingsHelper.FaceApiRoot) == string.Empty) needSettings = true;
            if (SettingsHelper.ReadSettings(SettingsHelper.FaceApiPersonGroup) == string.Empty) needSettings = true;
            if (SettingsHelper.ReadSettings(SettingsHelper.TimeBetweenPhotos) == string.Empty) needSettings = true;
            if (SettingsHelper.ReadSettings(SettingsHelper.MatchFoundPause) == string.Empty) needSettings = true;
            if (SettingsHelper.ReadSettings(SettingsHelper.FaceBoxSize) == string.Empty) needSettings = true;

            if (needSettings)
            {
                NavigationHelper.Navigate(typeof(SettingsPage));
            }
            else
            {
                NavigationHelper.Navigate(typeof(ManageMeetingsPage));
            }
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if(args.IsSettingsInvoked)
            {
                NavigationHelper.Navigate(typeof(SettingsPage));
            }
        }
    }
}

