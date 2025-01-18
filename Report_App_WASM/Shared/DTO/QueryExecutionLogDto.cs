namespace Report_App_WASM.Shared.DTO;

public class QueryExecutionLogDto : IDto
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
    public string? TotalDuration { get; set; }
    public string? SqlExcecutionDuration { get; set; }
    public string? DownloadDuration { get; set; }
    public int RowsFetched { get; set; }
    [MaxLength(250)] public string? RunBy { get; set; }
    [MaxLength(250)] public string? TypeJob { get; set; }
}