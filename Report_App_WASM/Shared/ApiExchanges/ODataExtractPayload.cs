namespace Report_App_WASM.Shared.ApiExchanges;

public class ODataExtractPayload
{
    public string? FunctionName { get; init; }
    public string? FilterValues { get; init; }
    public string? SortValues { get; init; }
    public string? FileName { get; init; }
    public string TabName { get; set; } = "Data";
    public int MaxResult { get; set; } = 100000;
}