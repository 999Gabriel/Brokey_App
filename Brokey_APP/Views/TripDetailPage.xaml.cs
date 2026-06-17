using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

// Detailseite einer einzelnen Reise: zeigt deren Gruppen/Ausgaben. IQueryAttributable empfängt die TripId aus der Navigation.
public partial class TripDetailPage : AnimatedContentPage, IQueryAttributable
{
    // Konstruktor: das TripDetailViewModel wird per Dependency Injection übergeben.
    public TripDetailPage(TripDetailViewModel viewModel)
    {
        // Baut das in TripDetailPage.xaml definierte UI auf.
        InitializeComponent();
        // Setzt den per DI injizierten ViewModel als BindingContext → XAML-Bindings greifen auf dessen Properties/Commands zu.
        BindingContext = viewModel;
    }

    // Wird von der Shell direkt nach der Navigation aufgerufen; das query-Dictionary enthält die von der TripsPage
    // übergebenen Parameter (z. B. TripId). Diese werden 1:1 an das ViewModel weitergereicht, das sie auswertet.
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (BindingContext is TripDetailViewModel viewModel)
        {
            viewModel.ApplyQueryAttributes(query);
        }
    }

    // Lifecycle-Event: wird aufgerufen, sobald die Seite sichtbar wird (auch bei Rückkehr von einer Unterseite).
    protected override void OnAppearing()
    {
        // Zuerst die Basis-Implementierung (Einblend-Animation aus AnimatedContentPage) ausführen.
        base.OnAppearing();

        // Wird beim Anzeigen der Seite aufgerufen → lädt im ViewModel die Trip-Details (Gruppen etc.) per TripService/API neu.
        if (BindingContext is TripDetailViewModel viewModel)
        {
            viewModel.LoadTripCommand.Execute(null);
        }
    }
}
