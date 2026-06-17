using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

// Seite zum Anlegen einer neuen Reise: Formular für Name, Datum, Währung usw., gebunden an das CreateTripViewModel.
public partial class CreateTripPage : AnimatedContentPage
{
    // Konstruktor: das CreateTripViewModel wird per Dependency Injection übergeben.
    public CreateTripPage(CreateTripViewModel viewModel)
    {
        // Baut das in CreateTripPage.xaml definierte UI auf.
        InitializeComponent();
        // Setzt den per DI injizierten ViewModel als BindingContext → XAML-Bindings greifen auf dessen Properties/Commands zu.
        BindingContext = viewModel;
    }
}
