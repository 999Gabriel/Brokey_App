namespace Brokey_APP.Models;

// Datencontainer für den Body beim Anlegen eines Trips (POST /api/trips), befüllt im CreateTripViewModel.
public class CreateTripRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }                       // Optionale Beschreibung (null erlaubt).
    public string BaseCurrency { get; set; } = "EUR";              // Hauptwährung des Trips, Standard "EUR".
    public DateTime StartDate { get; set; } = DateTime.Today;      // Vorbelegt mit heute.
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(3);   // Vorbelegt mit heute + 3 Tage.
}
