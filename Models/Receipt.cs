namespace Models;

// Beleg/Quittung als Bild (Tabelle "Receipts"), das zu einer Ausgabe hochgeladen wurde.
// Eine Ausgabe kann mehrere Belege haben (z.B. mehrere Fotos einer Rechnung).
public class Receipt
{
    public int Id { get; set; } // Primärschlüssel des Belegs.
    public int ExpenseId { get; set; } // Fremdschlüssel (FK) auf die zugehörige Ausgabe.
    public string ImagePath { get; set; } = string.Empty; // Dateipfad bzw. Speicherort des Belegbildes.
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow; // Zeitpunkt des Hochladens (Weltzeit/UTC).

    // ---- Navigation Properties ----
    public Expense Expense { get; set; } = null!; // Navigation (n:1): die Ausgabe zum Fremdschlüssel ExpenseId.
}
