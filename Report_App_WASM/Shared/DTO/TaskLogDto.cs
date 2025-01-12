﻿namespace Report_App_WASM.Shared.DTO;

public class TaskLogDto : IDto
{
    public long TaskLogId { get; set; }
    public long ScheduledTaskId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int DurationInSeconds { get; set; }
    public long DataProviderId { get; set; }
    [MaxLength(250)] public string? ProviderName { get; set; }
    [MaxLength(250)] public string? JobDescription { get; set; }
    [MaxLength(60)] public string? Type { get; set; }
    public string? Result { get; set; }
    public bool Error { get; set; }
    public bool HasSteps { get; set; }
    [MaxLength(250)] public string? MiscValue { get; set; }
    [MaxLength(200)] public string? RunBy { get; set; }
}