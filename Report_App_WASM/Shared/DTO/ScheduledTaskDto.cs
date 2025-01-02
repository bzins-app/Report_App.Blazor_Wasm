﻿namespace Report_App_WASM.Shared.DTO;

public sealed class ScheduledTaskDto : BaseTraceabilityDto, IDto
{
    public ScheduledTaskDto()
    {
        TaskQueries = new HashSet<ScheduledTaskQueryDto>();
        DistributionLists = new HashSet<ScheduledTaskDistributionListDto>();
    }

    public int ScheduledTaskId { get; set; }
    [Required][MaxLength(100)] public string? TaskName { get; set; }
    [Required][MaxLength(60)] public string ProviderName { get; set; }
    public int DataProviderId { get; set; }
    [MaxLength(60)] public string? TaskNamePrefix { get; set; }
    [MaxLength(100)] public string TimeZone { get; set; } = TimeZoneInfo.Local.Id;
    public TaskType Type { get; set; }

    [MaxLength(20)] public string? TypeName { get; set; }

    public FileType TypeFile { get; set; }

    [MaxLength(20)] public string? TypeFileName { get; set; }
    public bool IsEnabled { get; set; } = false;
    public bool SendByEmail { get; set; } = false;
    public int ReportsRetentionInDays { get; set; } = 90;
    [MaxLength(1000)] public string? Comment { get; set; }
    [MaxLength(1000)] public string Tags { get; set; } = "[]";
    [MaxLength(1000)] public string TaskHeaderParameters { get; set; } = "[]";
    [MaxLength(1000)] public string CronParameters { get; set; } = "[]";
    public bool UseGlobalQueryParameters { get; set; } = false;
    [MaxLength(1000)] public string QueryParameters { get; set; } = "[]";
    public DateTime? LastRunDateTime { get; set; } = null;
    public int FileStorageLocationId { get; set; }
    [MaxLength(1000)] public string MiscParamters { get; set; } = "[]";
    public ICollection<ScheduledTaskQueryDto> TaskQueries { get; set; } = new List<ScheduledTaskQueryDto>();
    public ICollection<ScheduledTaskDistributionListDto> DistributionLists { get; set; } = new List<ScheduledTaskDistributionListDto>();
    public DataProviderDto DataProvider { get; set; } = null!;
}