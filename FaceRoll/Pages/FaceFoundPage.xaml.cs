using FaceRoll.Model;
using FaceRoll.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FaceRoll.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FaceFoundPage : Page
    {
        public FaceFoundViewModel ViewModel
        {
            get { return DataContext as FaceFoundViewModel; }
        }

        public FaceFoundPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            await ViewModel.AddPersonAndContinue(e.Parameter as Identification);
        }
    }
}
