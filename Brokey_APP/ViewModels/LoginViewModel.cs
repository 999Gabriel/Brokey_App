using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public LoginViewModel()
    {
        Title = "Login";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter your email and password.";
            HasError = true;
            return;
        }

        IsBusy = true;
        HasError = false;

        try
        {
            // TODO: Call API for login
            await Task.Delay(500); // Simulated delay

            // Navigate to main app shell
            Application.Current!.Windows[0].Page = new AppShell();
        }
        catch (Exception)
        {
            ErrorMessage = "Login failed. Please try again.";
            HasError = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GoToRegisterAsync()
    {
        await Shell.Current.GoToAsync("//register");
    }

    [RelayCommand]
    private async Task ForgotPasswordAsync()
    {
        await Application.Current!.Windows[0].Page!.DisplayAlertAsync(
            "Forgot Password",
            "Password reset is not yet available. Please contact support.",
            "OK");
    }
}

