namespace API_Server.DTOs;

// Antwort von GET /api/trips – kompakte Trip-Übersicht für die Trips-Liste (Name, Zeitraum, Anzahlen).
public class TripSummaryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string BaseCurrency { get; set; } = string.Empty;
    public int CreatedById { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public int GroupCount { get; set; }
    public int MemberCount { get; set; }
}
