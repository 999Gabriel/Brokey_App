using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

public partial class GroupDetailPage : ContentPage, IQueryAttributable
{
    public GroupDetailPage(GroupDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (BindingContext is GroupDetailViewModel viewModel)
        {
            viewModel.ApplyQueryAttributes(query);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is GroupDetailViewModel viewModel)
        {
            viewModel.LoadMembersCommand.Execute(null);
        }
    }
}
