namespace Report_App_WASM.Server.Models;

public class DataProvider : BaseTraceability
{
    private string? _providerTypeName;
    public long DataProviderId { get; set; }
    [Required][MaxLength(250)] public string? ProviderName { get; set; }
    public ProviderType ProviderType { get; set; } = ProviderType.SourceDatabase;
    [MaxLength(20)]
    public string? ProviderTypeName
    {
        get => _providerTypeName;
        set
        {
            _providerTypeName = value;
            _providerTypeName = ProviderType.ToString();
        }
    }
    public bool IsEnabled { get; set; }
    public bool IsVisible { get; set; }
    [MaxLength(1000)] public string? ProviderIcon { get; set; } // Added MaxLength attribute
    [MaxLength(100)] public string? ProviderRoleId { get; set; }
    [MaxLength(100)] public string? TimeZone { get; set; } = TimeZoneInfo.Local.Id;
    [MaxLength(4000)] public string MiscParameters { get; set; } = "[]";
    public virtual ICollection<DatabaseConnection> DatabaseConnection { get; set; } = new List<DatabaseConnection>();
    public virtual ICollection<ScheduledTask> ScheduledTasks { get; set; } = new List<ScheduledTask>();
    public virtual ICollection<StoredQuery> StoredQueries { get; set; } = new List<StoredQuery>();
}