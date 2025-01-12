namespace Report_App_WASM.Shared.ApiExchanges;

public class UserConfigurations
{
    public long Id { get; set; }
    public string? SaveName { get; set; }
    public string? Parameters { get; set; }
    public TypeConfiguration TypeConfiguration { get; set; }
    public long IdIntConfiguration { get; set; }
    public string? IdStringConfiguration { get; set; }
    public string? SavedValues { get; set; }
}