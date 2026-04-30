using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

public partial class ProfilePage : AnimatedContentPage
{
    public ProfilePage(ProfileViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
