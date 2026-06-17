using ObjCRuntime;
using UIKit;

namespace Brokey_APP;

// iOS-spezifischer Start-Code (generierter Bootstrap): das eigentliche Programm-Einstiegstor unter iOS.
public class Program
{
    // Echter Programmeinstieg (main): übergibt die Kontrolle an iOS/UIKit und benennt das oben definierte AppDelegate.
    static void Main(string[] args)
    {
        // Hier könnte ein anderes Application-Delegate als "AppDelegate" angegeben werden – wir nutzen das Standard-Delegate.
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}