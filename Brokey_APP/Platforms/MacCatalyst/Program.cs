using ObjCRuntime;
using UIKit;

namespace Brokey_APP;

// Mac-Catalyst-spezifischer Start-Code (generierter Bootstrap): Programm-Einstiegstor unter macOS.
public class Program
{
    // Echter Programmeinstieg (main): übergibt die Kontrolle an UIKit und benennt das AppDelegate.
    static void Main(string[] args)
    {
        // Standard-Delegate "AppDelegate" wird verwendet; hier könnte man ein abweichendes angeben.
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}