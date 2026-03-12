using System.ComponentModel.DataAnnotations;

namespace API_Server.DTOs;

public class CreateTripRequest
{
    [Required]
    [StringLength(150, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string BaseCurrency { get; set; } = "EUR";

    public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime EndDate { get; set; } = DateTime.UtcNow.Date.AddDays(3);
}
