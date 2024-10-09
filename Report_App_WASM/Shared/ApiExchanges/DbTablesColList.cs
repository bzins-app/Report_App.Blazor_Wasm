namespace Report_App_WASM.Shared.ApiExchanges;

public class DbTablesColList
{
    public List<DescriptionValues> Values { get; set; } = new();
    public bool HasDescription { get; set; }
}