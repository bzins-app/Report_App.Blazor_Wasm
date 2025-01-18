namespace Report_App_WASM.Server.Models;

public class QueryExecutionLog : IExcludeAuditTrail
{
    public long Id { get; set; }
    [MaxLength(60)] public string? TypeDb { get; set; }
    [MaxLength(1000)] public string? Database { get; set; }
    public int CommandTimeOut { get; set; }
    public long DataProviderId { get; set; }
    [MaxLength(250)] public string? ProviderName { get; set; }
    public long TaskLogId { get; set; }
    public long ScheduledTaskId { get; set; }
    public long ScheduledTaskQueryId { get; set; }
    [MaxLength(1000)] public string? QueryName { get; set; }
    public string? Query { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime TransferBeginDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan SqlExcecutionDuration { get; set; }
    public TimeSpan DownloadDuration { get; set; }
    public int RowsFetched { get; set; }
    [MaxLength(250)] public string? RunBy { get; set; }
    [MaxLength(250)] public string? TypeJob { get; set; }
}