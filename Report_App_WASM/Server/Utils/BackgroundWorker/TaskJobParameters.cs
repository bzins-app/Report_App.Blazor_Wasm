namespace Report_App_WASM.Server.Utils.BackgroundWorker;

public class TaskJobParameters
{
    public long ScheduledTaskId { get; init; }
    public CancellationToken Cts { get; set; }
    public TaskType TaskType { get; init; }
    public List<EmailRecipient>? CustomEmails { get; init; } = null;
    public List<QueryCommandParameter>? QueryCommandParameters { get; init; } = new();
    public bool GenerateFiles { get; init; } = false;
    public bool ManualRun { get; init; } = false;
    public string? RunBy { get; init; } = "system";
}