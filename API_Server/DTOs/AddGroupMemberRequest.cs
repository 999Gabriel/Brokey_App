using System.ComponentModel.DataAnnotations;

namespace API_Server.DTOs;

public class AddGroupMemberRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Role { get; set; } = "Member";
}
