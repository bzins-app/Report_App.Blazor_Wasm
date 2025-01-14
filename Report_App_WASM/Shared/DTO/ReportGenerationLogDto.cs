﻿namespace Report_App_WASM.Shared.DTO;

public class ReportGenerationLogDto : IDto
{
    private double _fileSizeInMb;
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    [MaxLength(200)] public string? CreatedBy { get; set; }
    public long DataProviderId { get; set; }
    [MaxLength(250)] public string? ProviderName { get; set; }
    public long TaskLogId { get; set; }
    public long ScheduledTaskId { get; set; }
    [MaxLength(600)] public string? ReportName { get; set; }
    [MaxLength(600)] public string? SubName { get; set; }
    [MaxLength(60)] public string? FileType { get; set; }
    [MaxLength(600)] public string? FileName { get; set; }
    [MaxLength(1000)] public string? ReportPath { get; set; }
    public double FileSizeInMb
    {
        get => _fileSizeInMb;
        set => _fileSizeInMb = Math.Round(value, 2);
    }
    public bool IsAvailable { get; set; } = true;
    [MaxLength(4000)] public string? Result { get; set; }
    public bool Error { get; set; }
    public FileGenerationType? FileGenerationType { get; set; }
}