using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

public partial class RegisterPage : AnimatedContentPage
{
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
