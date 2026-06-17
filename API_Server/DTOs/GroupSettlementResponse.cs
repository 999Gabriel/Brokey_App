namespace API_Server.DTOs;

// Antwort von GET /api/groups/{id}/settlement – Abrechnung der Gruppe mit Balances und nötigen Transfers.
public class GroupSettlementResponse
{
    public int GroupId { get; set; }
    public int TripId { get; set; }
    public string Currency { get; set; } = "EUR";
    public List<SettlementBalanceResponse> Balances { get; set; } = [];
    public List<SettlementTransferResponse> Transfers { get; set; } = [];
}
