namespace Report_App_WASM.Shared.ApiExchanges;

public class DuplicateTask
{
    public long ScheduledTaskId { get; init; }
    public string? Name { get; init; }
}