using System.ComponentModel.DataAnnotations;

namespace API_Server.DTOs;

public class CreateGroupRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
}
