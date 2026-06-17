namespace Brokey_APP.Models;

// Ein Aufteilungs-Anteil einer Ausgabe (Teil von ExpenseResponse.Splits): zeigt, welcher Nutzer welchen Anteil
// trägt und ob/an wen er noch zahlen muss. Angezeigt in der Ausgaben-Detailseite.
public class ExpenseSplitResponse
{
    public int UserId { get; set; }                              // Nutzer, den dieser Anteil betrifft.
    public string Username { get; set; } = string.Empty;
    public decimal Amount { get; set; }                          // Sein rechnerischer Anteil an der Ausgabe.
    public int PaidToUserId { get; set; }                        // An wen der offene Betrag zu zahlen ist (i. d. R. der Zahler).
    public string PaidToUsername { get; set; } = string.Empty;
    public decimal OwesAmount { get; set; }                      // Wie viel dieser Nutzer noch schuldet.
    public bool IsPaidByUser { get; set; }                       // true, wenn dieser Nutzer die Ausgabe selbst bezahlt hat.
    public bool IsSettled { get; set; }                          // true, wenn der Anteil bereits beglichen ist.
    public DateTime? SettledAt { get; set; }                     // Zeitpunkt der Begleichung (null = noch offen).
}
