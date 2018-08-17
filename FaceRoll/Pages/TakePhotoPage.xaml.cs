using DataModels;
using FaceRoll.ViewModels;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FaceRoll.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TakePhotoPage : Page
    {
        public TakePhotoViewModel ViewModel
        {
            get { return DataContext as TakePhotoViewModel; }
        }

        public TakePhotoPage()
        {
            this.InitializeComponent();
            Application.Current.Suspending += Current_Suspending;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null)
            {
                App.ActiveMeeting = e.Parameter as Meeting;
            }

            ViewModel.Init(Dispatcher, PreviewControl, FacesCanvas);

            await ViewModel.StartPreviewAsync();
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            await ViewModel.CleanupCameraAsync();
        }

        private async void Current_Suspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await ViewModel.CleanupCameraAsync();
            deferral.Complete();
        }
    }
}

