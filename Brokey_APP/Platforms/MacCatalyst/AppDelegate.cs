using Foundation;

namespace Brokey_APP;

// Mac-Catalyst-spezifischer Start-Code (generierter Bootstrap; Catalyst = iOS-App, die unter macOS läuft).
// Das AppDelegate ist der Einstiegspunkt und erzeugt über MauiProgram die gemeinsame MAUI-App.
[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    // Baut die plattformunabhängige MAUI-App auf.
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}