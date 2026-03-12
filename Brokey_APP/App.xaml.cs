using Brokey_APP.Services;

namespace Brokey_APP;

public partial class App : Application
{
    private readonly ITokenStorageService _tokenStorageService;

    public App(ITokenStorageService tokenStorageService)
    {
        _tokenStorageService = tokenStorageService;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var isAuthenticated = _tokenStorageService.HasStoredToken();
        return new Window(isAuthenticated ? new AppShell() : new AuthShell());
    }
}
