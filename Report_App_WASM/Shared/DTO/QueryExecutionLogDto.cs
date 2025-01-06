namespace Report_App_WASM.Shared.DTO;

public class QueryExecutionLogDto : IDto
{
    public int Id { get; set; }
    [MaxLength(1000)] public string? TypeDb { get; set; }
    [MaxLength(1000)] public string? Database { get; set; }
    public int CommandTimeOut { get; set; }
    public int DataProviderId { get; set; }
    [MaxLength(250)] public string? ProviderName { get; set; }
    [MaxLength(1000)] public string? QueryName { get; set; }
    public string? Query { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime TransferBeginDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan SqlExcecutionDuration { get; set; }
    public TimeSpan DownloadDuration { get; set; }
    public int NbrOfRows { get; set; }
    [MaxLength(250)] public string? RunBy { get; set; }
    [MaxLength(250)] public string? TypeJob { get; set; }
}