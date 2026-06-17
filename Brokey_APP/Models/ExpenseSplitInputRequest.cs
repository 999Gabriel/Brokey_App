namespace Brokey_APP.Models;

// Ein einzelner Aufteilungs-Eintrag bei ungleicher Teilung (Teil von CreateExpenseRequest.SplitAllocations).
public class ExpenseSplitInputRequest
{
    public int UserId { get; set; }       // Der beteiligte Nutzer.
    public decimal Value { get; set; }    // Sein Anteil – je nach SplitMode ein fester Betrag oder ein Anteilswert.
}
