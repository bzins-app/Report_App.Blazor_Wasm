namespace Report_App_WASM.Server.Models;

public class UserPreferences : BaseTraceability
{
    public long Id { get; set; }
    [MaxLength(100)]public string? UserId { get; set; }
    [MaxLength(250)]public string? SaveName { get; set; }
    [MaxLength(4000)]public string? Parameters { get; set; }
    public TypeConfiguration TypeConfiguration { get; set; }
    public long IdIntConfiguration { get; set; }
    [MaxLength(100)]public string? IdStringConfiguration { get; set; }
    [MaxLength(4000)] public string? SavedValues { get; set; }
    [MaxLength(4000)] public string MiscParameters { get; set; } = "[]";
}