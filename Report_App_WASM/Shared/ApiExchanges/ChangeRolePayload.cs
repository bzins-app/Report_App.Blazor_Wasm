namespace Report_App_WASM.Shared.ApiExchanges;

public class ChangeRolePayload
{
    public string? UserName { get; init; }
    public IEnumerable<string>? Roles { get; init; }
}