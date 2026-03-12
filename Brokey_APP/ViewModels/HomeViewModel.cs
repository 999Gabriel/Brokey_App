using Brokey_APP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _welcomeMessage = "Hey there! Ready for your next adventure? 🧳";

    [ObservableProperty]
    private string _userName = "Traveler";

    public HomeViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Home";
        _ = LoadUserAsync();
    }

    private async Task LoadUserAsync()
    {
        try
        {
            var user = await _authService.GetCurrentUserAsync();
            UserName = user.Username;
        }
        catch
        {
            UserName = "Traveler";
        }
    }

    [RelayCommand]
    private async Task OpenTripsAsync()
    {
        await Shell.Current.GoToAsync("//trips");
    }

    [RelayCommand]
    private async Task OpenCreateTripAsync()
    {
        await Shell.Current.GoToAsync("create-trip");
    }
}
