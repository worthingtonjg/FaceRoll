using DataModels;
using FaceRoll.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FaceRoll.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ViewAttendancePage : Page
    {
        public ViewAttendanceViewModel ViewModel
        {
            get { return DataContext as ViewAttendanceViewModel; }
        }

        public ViewAttendancePage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter != null)
            {
                App.ActiveMeeting = e.Parameter as Meeting;
            }

            ViewModel.SetMeetingDetails(App.ActiveMeeting);
            await ViewModel.RefreshAttendees();
        }
    }
}
