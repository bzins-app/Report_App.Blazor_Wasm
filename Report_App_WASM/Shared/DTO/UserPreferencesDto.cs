namespace Report_App_WASM.Shared.DTO;

public class UserPreferencesDto : BaseTraceabilityDto, IDto
{
    public int Id { get; set; }
    [MaxLength(100)] public string? UserId { get; set; }
    [MaxLength(200)] public string? SaveName { get; set; }
    [MaxLength(1000)] public string? Parameters { get; set; }
    public TypeConfiguration TypeConfiguration { get; set; }
    public int IdIntConfiguration { get; set; }
    [MaxLength(100)] public string? IdStringConfiguration { get; set; }
    [MaxLength(1000)] public string? SavedValues { get; set; }
    [MaxLength(1000)] public string MiscParamters { get; set; } = "[]";
}