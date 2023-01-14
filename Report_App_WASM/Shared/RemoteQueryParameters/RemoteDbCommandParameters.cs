using Report_App_WASM.Shared.SerializedParameters;

namespace Report_App_WASM.Shared.RemoteQueryParameters;

public class RemoteDbCommandParameters
{
    public int ActivityId { get; init; }
    public string? FileName { get; set; }
    public string? QueryToRun { get; set; }
    public List<QueryCommandParameter>? QueryCommandParameters { get; init; } = new();
    public string? QueryInfo { get; init; }
    public bool FillDatatableSchema { get; set; } = false;
    public bool Test { get; init; } = false;
    public bool PaginatedResult { get; init; } = false;
    public int StartRecord { get; set; } = 0;
    public int MaxSize { get; set; } = 1000000;
    public DateTime LastRunDateTime { get; init; } = DateTime.Now;
}