namespace Brokey_APP.Models;

// Datencontainer für den Body beim Anlegen einer Gruppe (POST .../groups). Nur der Gruppenname wird benötigt.
public class CreateGroupRequest
{
    public string Name { get; set; } = string.Empty;
}
