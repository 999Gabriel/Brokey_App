using Brokey_APP.ViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace Brokey_APP.Views;

public partial class ExpenseDetailPage : AnimatedContentPage, IQueryAttributable
{
    public ExpenseDetailPage(ExpenseDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (BindingContext is ExpenseDetailViewModel viewModel)
            viewModel.ApplyQueryAttributes(query);
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ExpenseDetailViewModel.Expense))
            UpdateMapPin();
    }

    private void UpdateMapPin()
    {
        if (BindingContext is not ExpenseDetailViewModel vm) return;
        if (vm.Expense == null) return;
        if (!vm.Expense.HasLocation) return;

        DetailMap.Pins.Clear();

        var lat = (double)vm.Expense.Latitude!.Value;
        var lon = (double)vm.Expense.Longitude!.Value;
        var loc = new Location(lat, lon);

        DetailMap.Pins.Add(new Pin { Label = vm.Expense.Title, Location = loc });
        DetailMap.MoveToRegion(MapSpan.FromCenterAndRadius(loc, Distance.FromKilometers(1)));
    }
}
