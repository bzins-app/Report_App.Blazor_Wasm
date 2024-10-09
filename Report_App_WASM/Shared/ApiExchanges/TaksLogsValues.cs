namespace Report_App_WASM.Shared.ApiExchanges;

public class TaksLogsValues
{
    public DateTime Date { get; init; }
    public string? ActivityName { get; init; }
    public string? TypeTask { get; init; }
    public int TotalDuration { get; init; }
    public int NbrTasks { get; init; }
    public int NbrErrors { get; init; }
}