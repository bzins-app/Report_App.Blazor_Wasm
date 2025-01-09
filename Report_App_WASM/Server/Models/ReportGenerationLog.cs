﻿namespace Report_App_WASM.Server.Models;

public class ReportGenerationLog : IExcludeAuditTrail
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    [MaxLength(200)] public string? CreatedBy { get; set; }
    public int DataProviderId { get; set; }
    [MaxLength(250)] public string? ProviderName { get; set; }
    public int TaskLogId { get; set; }
    public int ScheduledTaskId { get; set; }
    [MaxLength(600)] public string? ReportName { get; set; }
    [MaxLength(600)] public string? SubName { get; set; }
    [MaxLength(60)] public string? FileType { get; set; }
    [MaxLength(600)] public string? FileName { get; set; }
    [MaxLength(1000)] public string? ReportPath { get; set; }
    public double FileSizeInMb { get; set; }
    public bool IsAvailable { get; set; } = true;
    [MaxLength(4000)] public string? Result { get; set; }
    public bool Error { get; set; }
}