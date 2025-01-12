namespace Report_App_WASM.Shared;

public class SelectItemDataproviderInfo
{
    public long DataProviderId { get; init; }
    public string? ProviderName { get; init; }
    public string? LogoPath { get; set; }
    public bool HasALogo { get; set; }
    public bool IsVisible { get; init; }
    public bool IsActivated { get; init; }
    public long DatabaseConnectionId { get; init; }
}