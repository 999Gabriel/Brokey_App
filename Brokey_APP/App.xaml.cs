namespace Brokey_APP;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Start with Auth flow (Login/Register)
        // After successful login, navigate to AppShell
        return new Window(new AuthShell());
    }
}