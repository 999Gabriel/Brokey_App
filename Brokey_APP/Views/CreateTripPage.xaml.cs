using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

public partial class CreateTripPage : AnimatedContentPage
{
    public CreateTripPage(CreateTripViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
