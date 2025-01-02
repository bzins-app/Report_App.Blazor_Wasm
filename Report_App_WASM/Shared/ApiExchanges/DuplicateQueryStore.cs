namespace Report_App_WASM.Shared.ApiExchanges;

public class DuplicateQueryStore
{
    public int QueryId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int DataProviderId { get; init; }
}