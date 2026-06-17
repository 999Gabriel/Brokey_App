namespace Brokey_APP.Models;

// Eine vorgeschlagene Ausgleichszahlung (Teil von GroupSettlementResponse.Transfers):
// "FromUser zahlt Amount an ToUser", damit am Ende alle Salden ausgeglichen sind.
public class SettlementTransferResponse
{
    public int FromUserId { get; set; }                          // Wer zahlt (Schuldner).
    public string FromUsername { get; set; } = string.Empty;
    public int ToUserId { get; set; }                            // Wer erhält (Gläubiger).
    public string ToUsername { get; set; } = string.Empty;
    public decimal Amount { get; set; }                          // Zu zahlender Betrag.
}
