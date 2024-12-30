namespace Report_App_WASM.Server.Models;

public class TaskStepLog : IExcludeAuditTrail
{
    public int Id { get; set; }
    public int TaskLogId { get; set; }
    public int TaskId { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.Now;
    [MaxLength(1000)] public string? Step { get; set; }
    [MaxLength(1000)] public string? Info { get; set; }
}