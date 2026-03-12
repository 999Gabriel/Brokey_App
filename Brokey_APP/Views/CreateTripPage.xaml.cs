using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

public partial class CreateTripPage : ContentPage
{
    public CreateTripPage(CreateTripViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
