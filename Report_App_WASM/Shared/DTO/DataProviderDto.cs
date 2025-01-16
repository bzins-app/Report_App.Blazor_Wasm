namespace Report_App_WASM.Shared.DTO;

public class DataProviderDto : BaseTraceabilityDto, IDto
{
    public long DataProviderId { get; set; }
    [Required] [MaxLength(250)] public string? ProviderName { get; set; }
    public ProviderType ProviderType { get; set; } = ProviderType.SourceDatabase;
    [MaxLength(20)] public string? ProviderTypeName { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsVisible { get; set; }
    [MaxLength(1000)] public string? ProviderIcon { get; set; } // Added MaxLength attribute
    [MaxLength(100)] public string? ProviderRoleId { get; set; }
    [MaxLength(100)] public string? TimeZone { get; set; } = TimeZoneInfo.Local.Id;
    [MaxLength(4000)] public string MiscParameters { get; set; } = "[]";

    public virtual ICollection<DatabaseConnectionDto> DatabaseConnection { get; set; } =
        new List<DatabaseConnectionDto>();

    public virtual ICollection<ScheduledTaskDto> ScheduledTasks { get; set; } = new List<ScheduledTaskDto>();
    public virtual ICollection<StoredQueryDto> StoredQueries { get; set; } = new List<StoredQueryDto>();
}