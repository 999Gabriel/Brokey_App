using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

// Detailseite einer Gruppe innerhalb einer Reise: zeigt Mitglieder/Ausgaben. IQueryAttributable empfängt die GroupId aus der Navigation.
public partial class GroupDetailPage : AnimatedContentPage, IQueryAttributable
{
    // Konstruktor: das GroupDetailViewModel wird per Dependency Injection übergeben.
    public GroupDetailPage(GroupDetailViewModel viewModel)
    {
        // Baut das in GroupDetailPage.xaml definierte UI auf.
        InitializeComponent();
        // Setzt den per DI injizierten ViewModel als BindingContext → XAML-Bindings greifen auf dessen Properties/Commands zu.
        BindingContext = viewModel;
    }

    // Wird von der Shell direkt nach der Navigation aufgerufen; das query-Dictionary enthält die von der TripDetailPage
    // übergebenen Parameter (z. B. GroupId). Diese werden 1:1 an das ViewModel weitergereicht, das sie auswertet.
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (BindingContext is GroupDetailViewModel viewModel)
        {
            viewModel.ApplyQueryAttributes(query);
        }
    }

    // Lifecycle-Event: wird aufgerufen, sobald die Seite sichtbar wird (auch bei Rückkehr von der AddMemberPage).
    protected override void OnAppearing()
    {
        // Zuerst die Basis-Implementierung (Einblend-Animation aus AnimatedContentPage) ausführen.
        base.OnAppearing();

        // Wird beim Anzeigen der Seite aufgerufen → startet im ViewModel das Laden der Gruppenmitglieder per TripService/API.
        if (BindingContext is GroupDetailViewModel viewModel)
        {
            viewModel.LoadMembersCommand.Execute(null);
        }
    }
}
