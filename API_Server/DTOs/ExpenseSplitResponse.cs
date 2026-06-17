namespace API_Server.DTOs;

// Teil von ExpenseResponse – ein berechneter Split (wer schuldet wem wie viel, Settled-Status).
public class ExpenseSplitResponse
{
    public int UserId { get; set; }                          // User, dem dieser Anteil zugeordnet ist
    public string Username { get; set; } = string.Empty;
    public decimal Amount { get; set; }                      // sein Anteil an der Gesamtausgabe
    public int PaidToUserId { get; set; }                    // an wen der Betrag fließt = der Zahler der Ausgabe
    public string PaidToUsername { get; set; } = string.Empty;
    public decimal OwesAmount { get; set; }                  // tatsächlich offener Betrag: 0, wenn der User selbst der Zahler ist
    public bool IsPaidByUser { get; set; }                   // true, wenn dieser User die Ausgabe bezahlt hat (schuldet sich nichts selbst)
    public bool IsSettled { get; set; }                      // true, wenn dieser Anteil bereits beglichen wurde
    public DateTime? SettledAt { get; set; }                 // Zeitpunkt der Begleichung (null solange offen)
}
