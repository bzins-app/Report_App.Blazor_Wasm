namespace Report_App_WASM.Server.Models;

public class ScheduledTaskQuery : BaseTraceability
{
    public int ScheduledTaskQueryId { get; set; }
    [Required][MaxLength(100)] public string? QueryName { get; set; }
    [Required] public string? Query { get; set; }
    [MaxLength(1000)] public string QueryParameters { get; set; } = "[]";
    [MaxLength(1000)] public string? ExecutionParameters { get; set; }
    public int ExecutionOrder { get; set; } = 99;
    public DateTime? LastRunDateTime { get; set; } = null;
    public int ExecutionCount { get; set; }
    public virtual ScheduledTask? ScheduledTask { get; set; }
}