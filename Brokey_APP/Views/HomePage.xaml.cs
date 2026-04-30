using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

public partial class HomePage : AnimatedContentPage
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
