using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Brokey_APP.WinUI;

// Windows-spezifischer Start-Code (generierter Bootstrap; WinUI = Windows-UI-Plattform).
// Diese App-Klasse ist der Windows-Einstiegspunkt und erzeugt die gemeinsame MAUI-App.
/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : MauiWinUIApplication
{
    // Konstruktor: erste eigene Code-Zeile unter Windows (entspricht main/WinMain). InitializeComponent()
    // lädt die zugehörige App.xaml.
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    // Baut die plattformunabhängige MAUI-App auf.
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}