using FaceRoll.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FaceRoll.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ManageMeetingsPage : Page
    {
        public ManageMeetingsViewModel ViewModel
        {
            get { return DataContext as ManageMeetingsViewModel;  }
        }

        public ManageMeetingsPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            App.ActiveMeeting = null;

            await ViewModel.RefreshMeetings();
        }
    }
}
