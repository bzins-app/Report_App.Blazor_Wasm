namespace Report_App_WASM.Shared.ApiExchanges;

public class TablesColsInfo
{
    public string TypeValue { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ColType { get; set; } = string.Empty;
    public int ColOrder { get; set; }
    public bool IsSnippet { get; set; }
}