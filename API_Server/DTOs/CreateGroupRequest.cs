using System.ComponentModel.DataAnnotations;

namespace API_Server.DTOs;

// Request-Body für POST /api/trips/{id}/groups – Eingabedaten (Name) zum Anlegen einer Gruppe.
public class CreateGroupRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
}
