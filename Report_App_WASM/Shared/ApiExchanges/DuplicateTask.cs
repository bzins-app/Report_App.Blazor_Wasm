namespace Report_App_WASM.Shared.ApiExchanges;

public class DuplicateTask
{
    public int TaskHeaderId { get; init; }
    public string? Name { get; init; }
}

public class DuplicateQueryStore
{
    public int QueryId { get; init; }
    public string? Name { get; init; }
}