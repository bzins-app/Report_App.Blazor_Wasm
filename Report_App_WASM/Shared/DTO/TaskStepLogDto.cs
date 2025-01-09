﻿namespace Report_App_WASM.Shared.DTO;

public class TaskStepLogDto : IDto
{
    public int Id { get; set; }
    public int TaskLogId { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.Now;
    [MaxLength(1000)] public string? Step { get; set; }
    public string? Info { get; set; }
    public LogType RelatedLogType { get; set; } = LogType.NotSet;
    public int RelatedLogId { get; set; }
}