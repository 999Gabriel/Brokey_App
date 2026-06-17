using Android.App;
using Android.Content.PM;
using Android.OS;

namespace Brokey_APP;

// Android-spezifischer Start-Code (generierter Bootstrap). Die MainActivity ist der erste sichtbare Bildschirm
// (Fenster) der App. Das [Activity]-Attribut konfiguriert sie:
//  - Theme: zeigt beim Start den Splash-Screen.
//  - MainLauncher = true: dies ist die App, die beim Antippen des Icons gestartet wird.
//  - LaunchMode SingleTop: verhindert mehrfaches Starten derselben Activity.
//  - ConfigurationChanges: bei diesen Änderungen (Drehen, Größe, Dark-Mode, ...) wird die Activity NICHT neu
//    erstellt, sondern nur informiert – das verhindert Flackern/Datenverlust beim z. B. Drehen des Geräts.
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    // Kein eigener Code nötig – die gesamte UI-Logik liegt in der gemeinsamen MAUI-App.
}