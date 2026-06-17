namespace Brokey_APP.Models;

// Datencontainer für den Body beim Hinzufügen eines Mitglieds (POST .../members), befüllt im AddMemberViewModel.
public class AddGroupMemberRequest
{
    public string UsernameOrEmail { get; set; } = string.Empty;   // Identifikation des Nutzers wahlweise per Username ODER E-Mail.
    public string Role { get; set; } = "Member";                  // Zugewiesene Rolle, Standard "Member".
}
