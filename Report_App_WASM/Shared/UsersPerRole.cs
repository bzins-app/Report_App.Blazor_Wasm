using System.ComponentModel.DataAnnotations;

namespace Report_App_WASM.Shared;

public class UsersPerRole
{
    [Key] public string? RoleName { get; set; }

    public string? UserName { get; set; }
}