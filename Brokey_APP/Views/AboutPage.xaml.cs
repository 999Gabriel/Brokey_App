using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

public partial class AboutPage : ContentPage
{
    public AboutPage(AboutViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

