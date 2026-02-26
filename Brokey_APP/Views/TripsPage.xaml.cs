using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

public partial class TripsPage : ContentPage
{
    public TripsPage(TripsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

