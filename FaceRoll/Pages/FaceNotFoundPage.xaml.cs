using FaceRoll.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FaceRoll.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FaceNotFoundPage : Page
    {
        public FaceNotFoundViewModel ViewModel
        {
            get { return DataContext as FaceNotFoundViewModel; }
        }

        public FaceNotFoundPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.CalledFromViewAttendance = (bool)e.Parameter;

            base.OnNavigatedTo(e);
        }
    }
}
