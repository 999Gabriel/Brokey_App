using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

// Übersichtsseite aller Reisen des Nutzers (Tab "Trips"): Liste der Trips, gebunden an das TripsViewModel.
public partial class TripsPage : AnimatedContentPage
{
    // Konstruktor: das TripsViewModel wird per Dependency Injection übergeben.
    public TripsPage(TripsViewModel viewModel)
    {
        // Baut das in TripsPage.xaml definierte UI auf.
        InitializeComponent();
        // Setzt den per DI injizierten ViewModel als BindingContext → XAML-Bindings greifen auf dessen Properties/Commands zu.
        BindingContext = viewModel;
    }

    // Lifecycle-Event: wird jedes Mal aufgerufen, wenn die Seite sichtbar wird (auch bei Rückkehr von einer Detailseite).
    protected override void OnAppearing()
    {
        // Zuerst die Basis-Implementierung (u. a. die Einblend-Animation aus AnimatedContentPage) ausführen.
        base.OnAppearing();

        // Wird beim Anzeigen der Seite aufgerufen → startet im ViewModel den Befehl, der die Trips per TripService/API neu lädt.
        if (BindingContext is TripsViewModel viewModel)
        {
            viewModel.LoadTripsCommand.Execute(null);
        }
    }
}
