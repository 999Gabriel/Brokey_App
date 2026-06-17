namespace API_Server.DTOs;

// Teil von GroupSettlementResponse – der Kontostand eines Users (bezahlt, Anteil, Netto-Saldo).
public class SettlementBalanceResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal TotalPaid { get; set; }   // Summe, die dieser User für die Gruppe ausgelegt hat
    public decimal TotalShare { get; set; }  // Summe seiner Anteile (was er rechnerisch hätte tragen müssen)
    public decimal NetBalance { get; set; }  // TotalPaid - TotalShare: positiv = bekommt Geld, negativ = schuldet Geld
}
