using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

// Profil-Seite: zeigt die Daten des eingeloggten Nutzers und die Logout-Option, gebunden an das ProfileViewModel.
public partial class ProfilePage : AnimatedContentPage
{
    // Konstruktor: das ProfileViewModel wird per Dependency Injection übergeben.
    public ProfilePage(ProfileViewModel viewModel)
    {
        // Baut das in ProfilePage.xaml definierte UI auf.
        InitializeComponent();
        // Setzt den per DI injizierten ViewModel als BindingContext → XAML-Bindings greifen auf dessen Properties/Commands zu.
        BindingContext = viewModel;
    }
}
