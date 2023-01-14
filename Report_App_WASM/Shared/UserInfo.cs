namespace Report_App_WASM.Shared;

public class UserInfo
{
    public bool IsAuthenticated { get; init; }
    public string? UserName { get; init; }
    public string? UserMail { get; init; }
    public string? Culture { get; set; }
    public string? AppTheme { get; init; }
    public List<ClaimsValue>? ExposedClaims { get; init; }
}

public class ClaimsValue
{
    public string? Type { get; init; }
    public string? Value { get; init; }
}