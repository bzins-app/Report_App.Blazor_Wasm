using Report_App_WASM.Shared.RemoteQueryParameters;

namespace Report_App_WASM.Shared.ApiExchanges;

public class RemoteDataPayload
{
    public RemoteDbCommandParameters? Values { get; init; }
    public bool LogPayload { get; init; } = true;
    public bool CalculateTotalElements { get; set; }
    public int QueryId { get; init; }
    public string? QueryName { get; init; }
    public string? ActivityName { get; init; }
}

public class DeleteTablePayload
{
    public string TableName { get; set; } = string.Empty;
    public int IdDataTransfer { get; set; }
}