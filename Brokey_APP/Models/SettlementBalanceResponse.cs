namespace Brokey_APP.Models;

public class SettlementBalanceResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal TotalPaid { get; set; }
    public decimal TotalShare { get; set; }
    public decimal NetBalance { get; set; }
}
