namespace Brokey_APP;

// Standard-Demoseite aus der MAUI-Projektvorlage (Klick-Zähler). In Brokey nicht aktiv genutzt,
// da die App über AppShell/AuthShell startet – dient nur als Beispiel/Platzhalter.
public partial class MainPage : ContentPage
{
    // Zähler für die Klicks auf den Button (Standard-MAUI-Demoseite).
    int count = 0;

    public MainPage()
    {
        // Lädt das XAML-Layout der Demoseite.
        InitializeComponent();
    }

    // Erhöht den Zähler bei jedem Klick und aktualisiert den Button-Text (mit Screenreader-Ansage).
    private void OnCounterClicked(object? sender, EventArgs e)
    {
        count++;

        if (count == 1)
            CounterBtn.Text = $"Clicked {count} time";
        else
            CounterBtn.Text = $"Clicked {count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }
}