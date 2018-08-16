using FaceRoll.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FaceRoll.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DrawWinnerPage : Page
    {
        public DrawWinnerViewModel ViewModel
        {
            get { return DataContext as DrawWinnerViewModel; }
        }

        public DrawWinnerPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            await ViewModel.DrawWinner();
        }
    }
}
