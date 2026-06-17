using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

// ViewModel des "About"-Screens (Info-Seite über die App). Erreichbar über den About-Tab/About-Eintrag.
// Zeigt nur statische Texte an (App-Beschreibung, Versionsnummer) und einen Link zum Impressum.
public partial class AboutViewModel : BaseViewModel
{
    // Statischer Beschreibungstext der App. Read-only Property (=>), wird im XAML als Label angezeigt.
    public string AppDescription => "Brokey is your travel companion that helps you and your friends " +
                                     "track every expense on your trips, holidays, and nights out. " +
                                     "Split costs fairly, upload receipts, and never lose track of who owes what.";

    // Fest hinterlegte Versionsnummer der App, wird in der View angezeigt.
    public string Version => "1.0.0";

    public AboutViewModel()
    {
        Title = "About";
    }

    // RelayCommand: Wird ausgelöst, wenn der User auf "Impressum" tippt.
    // Navigiert per Shell zur Impressum-Seite (registrierte Route "impressum").
    [RelayCommand]
    private async Task OpenImpressumAsync()
    {
        await Shell.Current.GoToAsync("impressum");
    }
}

