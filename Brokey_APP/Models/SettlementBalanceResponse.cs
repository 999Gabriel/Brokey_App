namespace Brokey_APP.Models;

// Der Saldo eines einzelnen Nutzers in der Abrechnung (Teil von GroupSettlementResponse.Balances).
public class SettlementBalanceResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal TotalPaid { get; set; }     // Summe, die dieser Nutzer tatsächlich bezahlt hat.
    public decimal TotalShare { get; set; }    // Summe, die er eigentlich tragen müsste (sein Anteil).
    public decimal NetBalance { get; set; }    // TotalPaid - TotalShare: positiv = bekommt Geld, negativ = schuldet Geld.
}
