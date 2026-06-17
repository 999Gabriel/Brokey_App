namespace Brokey_APP.Views;

// Gemeinsame Basisklasse für (fast) alle Seiten der App. Erbt von ContentPage und ergänzt eine einheitliche
// Einblend-/Hochfahr-Animation beim Anzeigen. Indem die Seiten von AnimatedContentPage statt ContentPage erben,
// bekommen sie diese Animation automatisch, ohne sie einzeln zu programmieren.
public class AnimatedContentPage : ContentPage
{
    // Merker, der verhindert, dass die Animation mehrfach gleichzeitig läuft.
    private bool _isAnimating;

    // Lifecycle-Event: wird aufgerufen, sobald die Seite sichtbar wird. "async void", weil unten auf die Animation gewartet wird.
    protected override async void OnAppearing()
    {
        // Basis-Implementierung der ContentPage zuerst ausführen.
        base.OnAppearing();

        // Abbrechen, wenn bereits eine Animation läuft oder die Seite (noch) keinen anzeigbaren Inhalt hat.
        if (_isAnimating || Content is not View root)
        {
            return;
        }

        // Markieren, dass jetzt animiert wird.
        _isAnimating = true;

        // Startzustand setzen: vollständig durchsichtig und 18 Pixel nach unten verschoben.
        root.Opacity = 0;
        root.TranslationY = 18;

        // Beide Animationen parallel ausführen und auf deren Ende warten:
        //  - FadeTo: Deckkraft in 280 ms von 0 auf 1 (langsam einblenden),
        //  - TranslateTo: in 320 ms an die Endposition (0,0) hochfahren. Easing.CubicOut = weiches Ausklingen.
        await Task.WhenAll(
            root.FadeToAsync(1, 280, Easing.CubicOut),
            root.TranslateToAsync(0, 0, 320, Easing.CubicOut));

        // Animation abgeschlossen → Merker zurücksetzen, damit sie beim nächsten Anzeigen erneut laufen kann.
        _isAnimating = false;
    }
}
