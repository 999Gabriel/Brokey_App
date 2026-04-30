using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

public partial class AddMemberPage : AnimatedContentPage, IQueryAttributable
{
    public AddMemberPage(AddMemberViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (BindingContext is AddMemberViewModel viewModel)
        {
            viewModel.ApplyQueryAttributes(query);
        }
    }
}
