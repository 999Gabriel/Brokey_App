using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _username = "Traveler";

    [ObservableProperty]
    private string _email = "traveler@brokey.app";

    [ObservableProperty]
    private string _baseCurrency = "EUR";

    public ProfileViewModel()
    {
        Title = "Profile";
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlertAsync(
            "Logout", "Are you sure you want to logout?", "Yes", "Cancel");

        if (confirm)
        {
            // TODO: Clear stored token
            Application.Current.Windows[0].Page = new AuthShell();
        }
    }
}

