namespace Report_App_WASM.Shared.ApiExchanges;

public class UserPayload
{
    public string? UserName { get; init; }
    public string? UserMail { get; init; }
    public string? Password { get; init; }
    public string? NewPassword { get; init; }
}