using Brokey_APP.Views;

namespace Brokey_APP;

// Haupt-Shell der eingeloggten App: definiert die Tab-Bar (in AppShell.xaml) und registriert die Detail-Routen.
public partial class AppShell : Shell
{
    public AppShell()
    {
        // Lädt das in AppShell.xaml definierte Layout (Tab-Bar und enthaltene Seiten).
        InitializeComponent();
        // Registriert alle Navigations-Routen der eingeloggten App (Detail-/Unterseiten außerhalb der Tab-Bar).
        // Pro Route wird ein kurzer Name einem Seiten-Typ zugeordnet; Shell.GoToAsync("<name>") öffnet dann die Seite.
        Routing.RegisterRoute("impressum", typeof(ImpressumPage));        // Rechtliche Impressum-Seite
        Routing.RegisterRoute("create-trip", typeof(CreateTripPage));      // Neue Reise anlegen
        Routing.RegisterRoute("trip-detail", typeof(TripDetailPage));      // Detailansicht einer Reise (mit GroupId/TripId-Parameter)
        Routing.RegisterRoute("group-detail", typeof(GroupDetailPage));    // Detailansicht einer Gruppe innerhalb einer Reise
        Routing.RegisterRoute("add-member", typeof(AddMemberPage));        // Mitglied zu einer Gruppe hinzufügen
        Routing.RegisterRoute("add-expense", typeof(AddExpensePage));      // Neue Ausgabe erfassen
        Routing.RegisterRoute("expense-detail", typeof(ExpenseDetailPage));// Detailansicht einer einzelnen Ausgabe
        Routing.RegisterRoute("trip-summary", typeof(TripSummaryPage));    // Abrechnungs-/Zusammenfassungsseite einer Reise
    }
}
