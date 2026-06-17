namespace Brokey_APP.Models;

// Die komplette Abrechnung einer Gruppe (Antwort von GET .../settlement): wer steht wie da (Balances) und
// welche Zahlungen würden alles ausgleichen (Transfers). Angezeigt im Abrechnungs-Bereich der Gruppen-Detailseite.
public class GroupSettlementResponse
{
    public int GroupId { get; set; }
    public int TripId { get; set; }
    public string Currency { get; set; } = "EUR";                              // Währung aller Beträge.
    public List<SettlementBalanceResponse> Balances { get; set; } = [];        // Saldo pro Nutzer (gezahlt vs. Anteil).
    public List<SettlementTransferResponse> Transfers { get; set; } = [];      // Vorgeschlagene Ausgleichszahlungen.
}
