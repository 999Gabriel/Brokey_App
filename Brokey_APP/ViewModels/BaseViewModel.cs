using CommunityToolkit.Mvvm.ComponentModel;

namespace Brokey_APP.ViewModels;

// Basis-Klasse für ALLE ViewModels der App. Erbt von ObservableObject (CommunityToolkit.Mvvm),
// wodurch die Klasse PropertyChanged-Benachrichtigungen an die UI senden kann (MVVM-Datenbindung).
// Stellt zwei gemeinsame Properties bereit, die fast jeder Screen braucht: IsBusy und Title.
public partial class BaseViewModel : ObservableObject
{
    // IsBusy = true während ein Lade-/Speichervorgang läuft. Die View bindet daran z.B. einen
    // Ladekreis (ActivityIndicator) und deaktiviert Buttons, damit nichts doppelt ausgelöst wird.
    // [ObservableProperty] erzeugt automatisch die öffentliche Property "IsBusy" + PropertyChanged.
    [ObservableProperty]
    private bool _isBusy;

    // Title = Überschrift des Screens (z.B. "Login", "My Trips"). Wird an die Seitentitel-Anzeige gebunden.
    [ObservableProperty]
    private string _title = string.Empty;
}

