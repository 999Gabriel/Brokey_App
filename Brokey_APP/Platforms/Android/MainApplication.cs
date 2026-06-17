using Android.App;
using Android.Runtime;

namespace Brokey_APP;

// Android-spezifischer Start-Code (generierter Bootstrap). Die [Application]-Klasse ist Androids zentraler
// Prozess-Einstiegspunkt; sie erzeugt über MauiProgram.CreateMauiApp() die gemeinsame MAUI-App.
[Application]
public class MainApplication : MauiApplication
{
    // Konstruktor, den die Android-Runtime selbst aufruft (handle/ownership verweisen auf das native Java-Objekt).
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    // Baut die plattformunabhängige MAUI-App auf.
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}