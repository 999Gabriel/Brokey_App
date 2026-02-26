using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Brokey_APP.ViewModels;

public partial class AboutViewModel : BaseViewModel
{
    public string AppDescription => "Brokey is your travel companion that helps you and your friends " +
                                     "track every expense on your trips, holidays, and nights out. " +
                                     "Split costs fairly, upload receipts, and never lose track of who owes what.";

    public string Version => "1.0.0";

    public AboutViewModel()
    {
        Title = "About";
    }

    [RelayCommand]
    private async Task OpenImpressumAsync()
    {
        await Shell.Current.GoToAsync("impressum");
    }
}

