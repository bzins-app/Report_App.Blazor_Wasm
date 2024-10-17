namespace Report_App_WASM.Shared.ApiExchanges;

public class DbTablesColList
{
    public List<TablesColsInfo> Values { get; set; } = new();
    public bool HasDescription { get; set; }
}