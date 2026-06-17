using Brokey_APP.ViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace Brokey_APP.Views;

// Detailseite einer einzelnen Ausgabe: zeigt Betrag/Beteiligte und – falls vorhanden – den Standort auf einer Karte.
// IQueryAttributable empfängt die ExpenseId aus der Navigation.
public partial class ExpenseDetailPage : AnimatedContentPage, IQueryAttributable
{
    // Konstruktor: das ExpenseDetailViewModel wird per Dependency Injection übergeben.
    public ExpenseDetailPage(ExpenseDetailViewModel viewModel)
    {
        // Baut das in ExpenseDetailPage.xaml definierte UI (inkl. der Karte "DetailMap") auf.
        InitializeComponent();
        // Setzt den per DI injizierten ViewModel als BindingContext → XAML-Bindings greifen auf dessen Properties/Commands zu.
        BindingContext = viewModel;
        // Abonniert das PropertyChanged-Ereignis des ViewModels: Die Karte ist kein bindbares Control, deshalb wird der
        // Pin manuell im Code-Behind gesetzt, sobald das ViewModel meldet, dass die Ausgabe geladen wurde.
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    // Wird von der Shell nach der Navigation aufgerufen; reicht den Parameter (z. B. ExpenseId) ans ViewModel weiter,
    // damit dieses die passende Ausgabe per TripService/API nachladen kann.
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (BindingContext is ExpenseDetailViewModel viewModel)
            viewModel.ApplyQueryAttributes(query);
    }

    // Event-Handler: feuert bei jeder Property-Änderung im ViewModel. Reagiert nur auf "Expense" (= Ausgabe geladen)
    // und aktualisiert dann den Karten-Pin.
    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ExpenseDetailViewModel.Expense))
            UpdateMapPin();
    }

    // Zeigt den gespeicherten Ausgabenstandort als Pin und zentriert die Karte darauf (falls vorhanden).
    private void UpdateMapPin()
    {
        // Abbrechen, wenn der BindingContext nicht das erwartete ViewModel ist.
        if (BindingContext is not ExpenseDetailViewModel vm) return;
        // Abbrechen, wenn noch keine Ausgabe geladen wurde.
        if (vm.Expense == null) return;
        // Abbrechen, wenn die Ausgabe keinen Standort hat (dann bleibt die Karte leer).
        if (!vm.Expense.HasLocation) return;

        // Eventuell vorhandene alte Pins entfernen, damit nur ein Pin angezeigt wird.
        DetailMap.Pins.Clear();

        // Koordinaten aus dem Modell holen ("!" weil HasLocation oben sicherstellt, dass sie gesetzt sind) und in Location umwandeln.
        var lat = (double)vm.Expense.Latitude!.Value;
        var lon = (double)vm.Expense.Longitude!.Value;
        var loc = new Location(lat, lon);

        // Neuen Pin mit dem Ausgaben-Titel setzen und die Karte mit 1 km Radius auf den Punkt zentrieren.
        DetailMap.Pins.Add(new Pin { Label = vm.Expense.Title, Location = loc });
        DetailMap.MoveToRegion(MapSpan.FromCenterAndRadius(loc, Distance.FromKilometers(1)));
    }
}
