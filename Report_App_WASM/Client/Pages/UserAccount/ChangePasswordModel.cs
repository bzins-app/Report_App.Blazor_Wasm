namespace Report_App_WASM.Client.Pages.UserAccount;

public class ChangePasswordModel
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public IdentityDefaultOptions? Options { get; set; }
}