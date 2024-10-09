namespace Report_App_WASM.Client.Pages.UserAccount;

public class UserFormModel
{
    public string? Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public IdentityDefaultOptions? Options { get; set; }
}