namespace Report_App_WASM.Shared.ApiExchanges;

public class ApiCrudPayload<T> where T : class?
{
    public string? UserName { get; init; } = "system";
    public T? EntityValue { get; init; }
}