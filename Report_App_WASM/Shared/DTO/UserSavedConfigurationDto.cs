namespace Report_App_WASM.Shared.DTO;

public class UserSavedConfigurationDto : BaseTraceabilityDto, IDto
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? SaveName { get; set; }
    public string? Parameters { get; set; }
    public TypeConfiguration TypeConfiguration { get; set; }
    public int IdIntConfiguration { get; set; }
    public string? IdStringConfiguration { get; set; }
    public string? SavedValues { get; set; }
}