namespace Report_App_WASM.Shared.ApiExchanges;

public class RemoteDataPayload
{
    public RemoteDbCommandParameters? Values { get; init; }
    public bool LogPayload { get; init; } = true;
    public bool CalculateTotalElements { get; set; }
    public long QueryId { get; init; }
    public string? QueryName { get; init; }
    public string? ProviderName { get; init; }
    public bool PivotTable { get; init; }
    public int PivotTableNbrColumns { get; init; }
    public string? ColumSorting { get; set; }
    public string? SortingDirection { get; set; }
}