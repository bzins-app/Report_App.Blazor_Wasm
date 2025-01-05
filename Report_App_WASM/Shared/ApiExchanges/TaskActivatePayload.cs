namespace Report_App_WASM.Shared.ApiExchanges;

public class TaskActivatePayload
{
    public bool Activate { get; init; }
    public int ScheduledTaskId { get; init; }
}