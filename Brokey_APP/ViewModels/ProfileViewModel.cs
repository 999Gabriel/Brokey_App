using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _username = "Traveler";

    [ObservableProperty]
    private string _email = "traveler@brokey.app";

    [ObservableProperty]
    private string _baseCurrency = "EUR";

    public ProfileViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Profile";

        _ = LoadUserInfoAsync();
    }

    private async Task LoadUserInfoAsync()
    {
        try
        {
            var user = await _authService.GetCurrentUserAsync();
            Username = user.Username;
            Email = user.Email;
            BaseCurrency = user.BaseCurrency;
        }
        catch
        {
            // Ignore – keep defaults
        }
    }

    [RelayCommand]
    private async Task RefreshProfileAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var user = await _authService.GetCurrentUserAsync();
            Username = user.Username;
            Email = user.Email;
            BaseCurrency = user.BaseCurrency;
        }
        catch
        {
            // Offline or error – keep cached data
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
        Application.Current!.Windows[0].Page = new AuthShell();
    }
}
