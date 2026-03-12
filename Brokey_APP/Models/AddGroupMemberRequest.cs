namespace Brokey_APP.Models;

public class AddGroupMemberRequest
{
    public string UsernameOrEmail { get; set; } = string.Empty;
    public string Role { get; set; } = "Member";
}
