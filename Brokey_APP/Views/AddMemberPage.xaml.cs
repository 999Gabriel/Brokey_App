using Brokey_APP.ViewModels;

namespace Brokey_APP.Views;

// Seite zum Hinzufügen eines Mitglieds zu einer Gruppe. IQueryAttributable empfängt die Ziel-GroupId aus der Navigation.
public partial class AddMemberPage : AnimatedContentPage, IQueryAttributable
{
    // Konstruktor: das AddMemberViewModel wird per Dependency Injection übergeben.
    public AddMemberPage(AddMemberViewModel viewModel)
    {
        // Baut das in AddMemberPage.xaml definierte UI auf.
        InitializeComponent();
        // Setzt den per DI injizierten ViewModel als BindingContext → XAML-Bindings greifen auf dessen Properties/Commands zu.
        BindingContext = viewModel;
    }

    // Wird von der Shell nach der Navigation aufgerufen; reicht die von der GroupDetailPage übergebenen Parameter
    // (z. B. GroupId, damit das ViewModel weiß, zu welcher Gruppe das Mitglied hinzugefügt wird) ans ViewModel weiter.
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (BindingContext is AddMemberViewModel viewModel)
        {
            viewModel.ApplyQueryAttributes(query);
        }
    }
}
