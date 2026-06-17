namespace Brokey_APP.Views;

// Impressum-Seite: rein statischer Rechtstext, hat daher kein ViewModel und keinen BindingContext.
public partial class ImpressumPage : AnimatedContentPage
{
    public ImpressumPage()
    {
        // Statische Impressum-Seite ohne ViewModel – lädt nur das in ImpressumPage.xaml definierte Layout.
        InitializeComponent();
    }
}
