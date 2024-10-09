namespace Report_App_WASM.Shared.DTO;

public sealed class TaskHeaderDto : BaseTraceabilityDto, IDto
{
    public TaskHeaderDto()
    {
        TaskDetails = new HashSet<TaskDetailDto>();
        TaskEmailRecipients = new HashSet<TaskEmailRecipientDto>();
    }

    public int TaskHeaderId { get; set; }

    [Required] [MaxLength(100)] public string TaskName { get; set; } = null!;

    [Required] [MaxLength(60)] public string ActivityName { get; set; } = null!;

    public int IdActivity { get; set; }

    [MaxLength(60)] public string? TaskNamePrefix { get; set; }

    public TaskType Type { get; set; }

    [MaxLength(20)] public string? TypeName { get; set; }

    public FileType TypeFile { get; set; }

    [MaxLength(20)] public string? TypeFileName { get; set; }
    public string Tags { get; set; } = "[]";
    public bool IsActivated { get; set; } = false;
    public bool SendByEmail { get; set; } = false;
    public int ReportsRetentionInDays { get; set; } = 90;
    public string? Comment { get; set; }
    public string TaskHeaderParameters { get; set; } = "[]";
    public string CronParameters { get; set; } = "[]";
    public bool UseGlobalQueryParameters { get; set; } = false;
    public string QueryParameters { get; set; } = "[]";
    public DateTime? LastRunDateTime { get; set; } = null;
    public int FileDepositPathConfigurationId { get; set; }
    public ICollection<TaskDetailDto> TaskDetails { get; set; } = new List<TaskDetailDto>();
    public ICollection<TaskEmailRecipientDto> TaskEmailRecipients { get; set; } = new List<TaskEmailRecipientDto>();
    public ActivityDto Activity { get; set; } = null!;
}