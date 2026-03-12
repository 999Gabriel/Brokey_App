using Brokey_APP.Views;

namespace Brokey_APP;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("impressum", typeof(ImpressumPage));
        Routing.RegisterRoute("create-trip", typeof(CreateTripPage));
        Routing.RegisterRoute("trip-detail", typeof(TripDetailPage));
        Routing.RegisterRoute("group-detail", typeof(GroupDetailPage));
        Routing.RegisterRoute("add-member", typeof(AddMemberPage));
    }
}
