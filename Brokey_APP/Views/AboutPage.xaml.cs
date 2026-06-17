using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

// "Über die App"-Seite: zeigt App-Infos und einen Link zum Impressum, gebunden an das AboutViewModel.
public partial class AboutPage : AnimatedContentPage
{
    // Konstruktor: das AboutViewModel wird per Dependency Injection übergeben.
    public AboutPage(AboutViewModel viewModel)
    {
        // Baut das in AboutPage.xaml definierte UI auf.
        InitializeComponent();
        // Setzt den per DI injizierten ViewModel als BindingContext → XAML-Bindings greifen auf dessen Properties/Commands zu.
        BindingContext = viewModel;
    }
}
