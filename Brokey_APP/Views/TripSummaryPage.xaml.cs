using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

// Zusammenfassungs-/Abrechnungsseite einer Reise: zeigt, wer wem wie viel schuldet, gebunden an das TripSummaryViewModel.
public partial class TripSummaryPage : AnimatedContentPage
{
    // Konstruktor: das TripSummaryViewModel wird per Dependency Injection übergeben.
    public TripSummaryPage(TripSummaryViewModel viewModel)
    {
        // Baut das in TripSummaryPage.xaml definierte UI auf.
        InitializeComponent();
        // Setzt den per DI injizierten ViewModel als BindingContext → XAML-Bindings greifen auf dessen Properties/Commands zu.
        BindingContext = viewModel;
    }
}
