using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

public partial class TripsPage : ContentPage
{
    public TripsPage(TripsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is TripsViewModel viewModel)
        {
            viewModel.LoadTripsCommand.Execute(null);
        }
    }
}
