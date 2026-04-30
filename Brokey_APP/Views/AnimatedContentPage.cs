namespace Brokey_APP.Views;

public class AnimatedContentPage : ContentPage
{
    private bool _isAnimating;

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isAnimating || Content is not View root)
        {
            return;
        }

        _isAnimating = true;

        root.Opacity = 0;
        root.TranslationY = 18;

        await Task.WhenAll(
            root.FadeToAsync(1, 280, Easing.CubicOut),
            root.TranslateToAsync(0, 0, 320, Easing.CubicOut));

        _isAnimating = false;
    }
}
