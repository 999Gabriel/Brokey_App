using Brokey_APP.ViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace Brokey_APP.Views;

public partial class AddExpensePage : AnimatedContentPage, IQueryAttributable
{
    public AddExpensePage(AddExpenseViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (BindingContext is AddExpenseViewModel viewModel)
            viewModel.ApplyQueryAttributes(query);
    }

    // Sync map pin & camera whenever coordinates change in the VM
    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(AddExpenseViewModel.SelectedLatitude)
                           or nameof(AddExpenseViewModel.SelectedLongitude))
        {
            UpdateMapPin();
        }
        else if (e.PropertyName is nameof(AddExpenseViewModel.AddLocation))
        {
            if (BindingContext is AddExpenseViewModel vm && vm.AddLocation)
                InitialiseMap();
        }
    }

    // Move the map to a sensible default region the first time the section is shown
    private void InitialiseMap()
    {
        if (BindingContext is not AddExpenseViewModel vm) return;

        if (vm.SelectedLatitude.HasValue && vm.SelectedLongitude.HasValue)
        {
            UpdateMapPin();
        }
        else
        {
            // Default to a world-level view so the map renders rather than showing a blank tile
            ExpenseMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(48.8566, 2.3522),   // Paris — visible landmark, easy to zoom out from
                Distance.FromKilometers(500)));
        }
    }

    private void UpdateMapPin()
    {
        if (BindingContext is not AddExpenseViewModel vm) return;

        ExpenseMap.Pins.Clear();

        if (!vm.SelectedLatitude.HasValue || !vm.SelectedLongitude.HasValue) return;

        var loc = new Location(vm.SelectedLatitude.Value, vm.SelectedLongitude.Value);
        ExpenseMap.Pins.Add(new Pin { Label = "Expense location", Location = loc });
        ExpenseMap.MoveToRegion(MapSpan.FromCenterAndRadius(loc, Distance.FromKilometers(1)));
    }

    private void OnMapClicked(object? sender, MapClickedEventArgs e)
    {
        if (BindingContext is not AddExpenseViewModel vm) return;
        vm.SetLocation(e.Location.Latitude, e.Location.Longitude);
    }
}
