namespace Brokey_APP;

// Shell für den nicht eingeloggten Zustand: enthält nur Login- und Registrierungs-Seite (siehe AuthShell.xaml).
// Wird von App.CreateWindow gewählt, wenn kein gültiges Token gespeichert ist.
public partial class AuthShell : Shell
{
    public AuthShell()
    {
        // Lädt das in AuthShell.xaml definierte Layout (Login/Registrierung). Hier sind keine Detail-Routen nötig.
        InitializeComponent();
    }
}
