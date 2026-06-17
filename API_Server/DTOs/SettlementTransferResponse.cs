namespace API_Server.DTOs;

// Teil von GroupSettlementResponse – ein nötiger Ausgleichstransfer "User A zahlt User B den Betrag X".
public class SettlementTransferResponse
{
    public int FromUserId { get; set; }
    public string FromUsername { get; set; } = string.Empty;
    public int ToUserId { get; set; }
    public string ToUsername { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
