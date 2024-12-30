namespace Report_App_WASM.Shared;

public class SelectItemDataproviderInfo
{
    public int DataProviderId { get; init; }
    public string? ProviderName { get; init; }
    public string? LogoPath { get; set; }
    public bool HasALogo { get; set; }
    public bool IsVisible { get; init; }
    public bool IsActivated { get; init; }
    public int DatabaseConnectionId { get; init; }
}