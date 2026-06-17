using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

// Registrierungs-Seite (Teil der AuthShell): Formular für ein neues Benutzerkonto, gebunden an das RegisterViewModel.
public partial class RegisterPage : AnimatedContentPage
{
    // Konstruktor: das RegisterViewModel wird per Dependency Injection übergeben.
    public RegisterPage(RegisterViewModel viewModel)
    {
        // Baut das in RegisterPage.xaml definierte UI auf.
        InitializeComponent();
        // Setzt den per DI injizierten ViewModel als BindingContext → XAML-Bindings greifen auf dessen Properties/Commands zu.
        BindingContext = viewModel;
    }
}
