namespace Brokey_APP.Models;

public class GroupSettlementResponse
{
    public int GroupId { get; set; }
    public int TripId { get; set; }
    public string Currency { get; set; } = "EUR";
    public List<SettlementBalanceResponse> Balances { get; set; } = [];
    public List<SettlementTransferResponse> Transfers { get; set; } = [];
}
