namespace Report_App_WASM.Shared.DTO;

public sealed class ScheduledTaskDto : BaseTraceabilityDto, IDto
{
    public ScheduledTaskDto()
    {
        TaskQueries = new HashSet<ScheduledTaskQueryDto>();
        DistributionLists = new HashSet<ScheduledTaskDistributionListDto>();
    }

    public long ScheduledTaskId { get; set; }
    [Required] [MaxLength(200)] public string? TaskName { get; set; }
    [Required] [MaxLength(250)] public string ProviderName { get; set; }
    public long IdDataProvider { get; set; }
    [MaxLength(60)] public string? TaskNamePrefix { get; set; }
    public TaskType Type { get; set; }
    [MaxLength(20)] public string? TypeName { get; set; }
    public FileType TypeFile { get; set; }
    [MaxLength(20)] public string? TypeFileName { get; set; }
    public bool IsEnabled { get; set; } = false;
    public bool SendByEmail { get; set; } = false;
    public int ReportsRetentionInDays { get; set; } = 90;
    [MaxLength(1000)] public string? Comment { get; set; }
    [MaxLength(1000)] public string Tags { get; set; } = "[]";
    [MaxLength(4000)] public string TaskParameters { get; set; } = "[]";
    [MaxLength(4000)] public string CronParameters { get; set; } = "[]";
    public bool UseGlobalQueryParameters { get; set; } = false;
    [MaxLength(4000)] public string GlobalQueryParameters { get; set; } = "[]";
    [MaxLength(100)] public string? TimeZone { get; set; } = TimeZoneInfo.Local.Id;
    public DateTime? LastRunDateTime { get; set; } = null;
    public long FileStorageLocationId { get; set; }
    [MaxLength(4000)] public string MiscParameters { get; set; } = "[]";
    public ICollection<ScheduledTaskQueryDto> TaskQueries { get; set; } = new List<ScheduledTaskQueryDto>();

    public ICollection<ScheduledTaskDistributionListDto> DistributionLists { get; set; } =
        new List<ScheduledTaskDistributionListDto>();

    public DataProviderDto DataProvider { get; set; } = null!;
}