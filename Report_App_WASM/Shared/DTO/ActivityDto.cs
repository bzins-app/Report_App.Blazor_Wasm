namespace Report_App_WASM.Shared.DTO;

public class ActivityDto : BaseTraceabilityDto, IDto
{
    public int ActivityId { get; set; }

    [Required] [MaxLength(60)] public string? ActivityName { get; set; }

    public ActivityType ActivityType { get; set; } = ActivityType.SourceDb;

    [MaxLength(20)] public string? ActivityTypeName { get; set; }

    public bool IsActivated { get; set; }
    public bool Display { get; set; }
    public string? ActivityLogo { get; set; }

    [MaxLength(60)] public string? ActivityRoleId { get; set; }

    public virtual ICollection<ActivityDbConnectionDto> ActivityDbConnections { get; set; } =
        new List<ActivityDbConnectionDto>();

    public virtual ICollection<TaskHeaderDto> TaskHeaders { get; set; } = new List<TaskHeaderDto>();
    public virtual ICollection<QueryStoreDto> QueryStores { get; set; } = new List<QueryStoreDto>();
}