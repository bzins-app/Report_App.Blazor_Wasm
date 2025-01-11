namespace Report_App_WASM.Shared.ApiExchanges;

public class DuplicateQueryStore
{
    public long QueryId { get; init; }
    public string Name { get; init; } = string.Empty;
    public long DataProviderId { get; init; }
}