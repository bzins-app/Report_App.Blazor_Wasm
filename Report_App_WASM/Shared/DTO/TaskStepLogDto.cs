namespace Report_App_WASM.Shared.DTO;

public class TaskStepLogDto : IDto
{
    public long Id { get; set; }
    public long TaskLogId { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.Now;
    [MaxLength(1000)] public string? Step { get; set; }
    public string? Info { get; set; }
    public LogType RelatedLogType { get; set; } = LogType.NotSet;
    public long RelatedLogId { get; set; }
}