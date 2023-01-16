using Report_App_WASM.Shared.RemoteQueryParameters;

namespace Report_App_WASM.Shared.ApiExchanges;

public class RemoteDataPayload
{
    public RemoteDbCommandParameters? Values { get; init; }
    public bool CalculateTotalElements { get; set; }
    public int QueryId { get; init; }
    public string? QueryName { get; init; }
    public string? ActivityName { get; init; }
    public bool PivotTable { get; init; }
    public int PivotTableNbrColumns { get; init; }
}