namespace Report_App_WASM.Shared;

public class SelectItemActivitiesInfo
{
    public int ActivityId { get; init; }
    public string? ActivityName { get; init; }
    public string? LogoPath { get; set; }
    public bool HasALogo { get; set; }
    public bool IsVisible { get; init; }
    public bool IsActivated { get; init; }
    public int DbConnectionId { get; init; }
}

public class SelectItem
{
    public int Id { get; init; }
    public string? Name { get; init; }
}