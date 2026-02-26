using CommunityToolkit.Mvvm.ComponentModel;

namespace Brokey_APP.ViewModels;

public partial class TripsViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _emptyMessage = "No trips yet! Tap + to plan your first adventure.";

    public TripsViewModel()
    {
        Title = "My Trips";
    }
}

