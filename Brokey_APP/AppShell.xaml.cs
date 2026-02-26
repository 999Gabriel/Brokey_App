using Brokey_APP.Views;

namespace Brokey_APP;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("impressum", typeof(ImpressumPage));
    }
}