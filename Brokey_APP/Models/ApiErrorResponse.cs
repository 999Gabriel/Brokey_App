namespace Brokey_APP.Models;

// Datencontainer für die einheitliche Fehlerantwort der API. Wird in den Services bei Fehler-Statuscodes
// aus dem Body gelesen, um dem Nutzer die konkrete Server-Meldung anzuzeigen.
public class ApiErrorResponse
{
    public string Message { get; set; } = string.Empty;
}

