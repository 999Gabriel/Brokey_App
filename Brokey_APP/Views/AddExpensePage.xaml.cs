using Brokey_APP.ViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace Brokey_APP.Views;

// Seite zum Erfassen einer neuen Ausgabe: Formular plus optionale Karte zur Standortauswahl.
// IQueryAttributable empfängt die Ziel-GroupId aus der Navigation. Gebunden an das AddExpenseViewModel.
public partial class AddExpensePage : AnimatedContentPage, IQueryAttributable
{
    // Konstruktor: das AddExpenseViewModel wird per Dependency Injection übergeben.
    public AddExpensePage(AddExpenseViewModel viewModel)
    {
        // Baut das in AddExpensePage.xaml definierte UI (inkl. der Karte "ExpenseMap") auf.
        InitializeComponent();
        // Setzt den per DI injizierten ViewModel als BindingContext → XAML-Bindings greifen auf dessen Properties/Commands zu.
        BindingContext = viewModel;
        // Abonniert PropertyChanged: Die Karte ist nicht bindbar, daher wird sie im Code-Behind synchron gehalten,
        // sobald sich im ViewModel die Koordinaten oder die Standort-Option ändern.
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    // Wird von der Shell nach der Navigation aufgerufen; reicht z. B. die GroupId ans ViewModel weiter,
    // damit die neue Ausgabe der richtigen Gruppe zugeordnet wird.
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (BindingContext is AddExpenseViewModel viewModel)
            viewModel.ApplyQueryAttributes(query);
    }

    // Event-Handler: feuert bei jeder Property-Änderung im ViewModel und hält die Karte mit dem ViewModel synchron.
    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Hat sich eine der gewählten Koordinaten geändert → Pin/Kamera der Karte neu setzen.
        if (e.PropertyName is nameof(AddExpenseViewModel.SelectedLatitude)
                           or nameof(AddExpenseViewModel.SelectedLongitude))
        {
            UpdateMapPin();
        }
        // Wurde die Standort-Eingabe gerade eingeschaltet (Schalter "AddLocation") → Karte erstmalig initialisieren.
        else if (e.PropertyName is nameof(AddExpenseViewModel.AddLocation))
        {
            // Nur initialisieren, wenn der Schalter tatsächlich auf "an" steht.
            if (BindingContext is AddExpenseViewModel vm && vm.AddLocation)
                InitialiseMap();
        }
    }

    // Zeigt beim ersten Öffnen des Karten-Bereichs entweder den bereits gewählten Punkt oder eine sinnvolle Standardregion.
    private void InitialiseMap()
    {
        if (BindingContext is not AddExpenseViewModel vm) return;

        // Wenn bereits Koordinaten gewählt sind, direkt diesen Punkt anzeigen.
        if (vm.SelectedLatitude.HasValue && vm.SelectedLongitude.HasValue)
        {
            UpdateMapPin();
        }
        else
        {
            // Sonst eine weiträumige Standardansicht zeigen, damit die Karte sichtbar rendert statt leer zu bleiben.
            ExpenseMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(48.8566, 2.3522),   // Paris – gut erkennbarer Bezugspunkt, von dem aus man leicht herauszoomt
                Distance.FromKilometers(500)));
        }
    }

    // Setzt einen Pin auf die im ViewModel gewählten Koordinaten und zentriert die Karte darauf.
    private void UpdateMapPin()
    {
        if (BindingContext is not AddExpenseViewModel vm) return;

        // Alte Pins entfernen, damit immer nur der aktuell gewählte Punkt angezeigt wird.
        ExpenseMap.Pins.Clear();

        // Abbrechen, wenn (noch) kein Standort gewählt wurde.
        if (!vm.SelectedLatitude.HasValue || !vm.SelectedLongitude.HasValue) return;

        // Gewählten Punkt als Pin setzen und die Karte mit 1 km Radius darauf zentrieren.
        var loc = new Location(vm.SelectedLatitude.Value, vm.SelectedLongitude.Value);
        ExpenseMap.Pins.Add(new Pin { Label = "Expense location", Location = loc });
        ExpenseMap.MoveToRegion(MapSpan.FromCenterAndRadius(loc, Distance.FromKilometers(1)));
    }

    // Event-Handler für Tippen auf die Karte: übernimmt die angetippte Position als gewählten Ausgabenstandort.
    // Das ViewModel speichert die Koordinaten, was wiederum PropertyChanged auslöst und so den Pin aktualisiert.
    private void OnMapClicked(object? sender, MapClickedEventArgs e)
    {
        if (BindingContext is not AddExpenseViewModel vm) return;
        vm.SetLocation(e.Location.Latitude, e.Location.Longitude);
    }
}
