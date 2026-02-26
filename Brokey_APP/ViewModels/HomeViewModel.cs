using CommunityToolkit.Mvvm.ComponentModel;

namespace Brokey_APP.ViewModels;

public partial class HomeViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _welcomeMessage = "Hey there! Ready for your next adventure? 🧳";

    [ObservableProperty]
    private string _userName = "Traveler";

    public HomeViewModel()
    {
        Title = "Home";
    }
}

