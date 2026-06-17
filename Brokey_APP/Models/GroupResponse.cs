namespace Brokey_APP.Models;

// Eine Gruppe innerhalb eines Trips (Antwort von GET/POST .../groups). Angezeigt als Kachel auf der Trip-Detailseite.
public class GroupResponse
{
    public int Id { get; set; }
    public int TripId { get; set; }                  // Zugehöriger Trip (Fremdschlüssel).
    public string Name { get; set; } = string.Empty;
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }             // Vorberechnete Anzahl Mitglieder der Gruppe.
    public int ExpenseCount { get; set; }            // Vorberechnete Anzahl Ausgaben der Gruppe.
    public decimal TotalExpenseAmount { get; set; }  // Summe der Ausgaben dieser Gruppe.
}
