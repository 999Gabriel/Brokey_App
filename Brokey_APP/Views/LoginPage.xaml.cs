using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

// Login-Seite (Teil der AuthShell): zeigt Eingabefelder für E-Mail/Passwort, gebunden an das LoginViewModel.
public partial class LoginPage : AnimatedContentPage
{
    // Konstruktor: das LoginViewModel wird per Dependency Injection (in MauiProgram registriert) übergeben.
    public LoginPage(LoginViewModel viewModel)
    {
        // Baut das in LoginPage.xaml definierte UI auf.
        InitializeComponent();
        // Setzt den per DI injizierten ViewModel als BindingContext → XAML-Bindings greifen auf dessen Properties/Commands zu.
        BindingContext = viewModel;
    }
}
