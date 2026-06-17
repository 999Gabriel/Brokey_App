using Foundation;

namespace Brokey_APP;

// iOS-spezifischer Start-Code (generierter Bootstrap). Das AppDelegate ist der von iOS erwartete Einstiegspunkt;
// es erzeugt über MauiProgram.CreateMauiApp() die gemeinsame MAUI-App (DI, Services, Routen).
[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    // Baut die plattformunabhängige MAUI-App auf.
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}