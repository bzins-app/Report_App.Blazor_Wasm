namespace Report_App_WASM.Server.Models;

public class StoredQuery : BaseTraceability
{
    public int Id { get; set; }
    public int IdDataProvider { get; set; }
    [MaxLength(250)] public string? ProviderName { get; set; }
    [MaxLength(1000)] public string? Comment { get; set; }
    [MaxLength(1000)] public string Tags { get; set; } = "[]";
    [MaxLength(200)] public string? QueryName { get; set; }
    public string? Query { get; set; }
    [MaxLength(4000)] public string Parameters { get; set; } = "[]";
    [MaxLength(4000)] public string QueryParameters { get; set; } = "[]";
    [MaxLength(4000)] public string MiscParamters { get; set; } = "[]";
    public virtual DataProvider? DataProvider { get; set; }
}
