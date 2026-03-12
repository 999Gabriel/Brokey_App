using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

public partial class TripDetailPage : ContentPage, IQueryAttributable
{
    public TripDetailPage(TripDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (BindingContext is TripDetailViewModel viewModel)
        {
            viewModel.ApplyQueryAttributes(query);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is TripDetailViewModel viewModel)
        {
            viewModel.LoadTripCommand.Execute(null);
        }
    }
}
