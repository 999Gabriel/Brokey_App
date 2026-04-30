using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

public partial class TripSummaryPage : AnimatedContentPage
{
    public TripSummaryPage(TripSummaryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
