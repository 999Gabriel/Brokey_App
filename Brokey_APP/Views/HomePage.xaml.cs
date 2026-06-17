using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

// Startseite (Dashboard) der eingeloggten App: erster Tab, gebunden an das HomeViewModel.
public partial class HomePage : AnimatedContentPage
{
    // Konstruktor: das HomeViewModel wird per Dependency Injection übergeben.
    public HomePage(HomeViewModel viewModel)
    {
        // Baut das in HomePage.xaml definierte UI auf.
        InitializeComponent();
        // Setzt den per DI injizierten ViewModel als BindingContext → XAML-Bindings greifen auf dessen Properties/Commands zu.
        BindingContext = viewModel;
    }
}
