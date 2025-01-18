namespace Report_App_WASM.Shared.ExternalApi;

public class TasksInfo
{
    public long TaksId { get; set; }
    public string? ActivityName { get; set; }
    public string? TaskName { get; set; }
    public string? TaskType { get; set; }
    public bool IsActivated { get; set; }
    public bool HasADepositPath { get; set; }
    public string? TypeOfGeneratedFile { get; set; }
    public DateTime? LastRunDateTime { get; set; }
    public List<string?> QueriesName { get; set; } = new();
}