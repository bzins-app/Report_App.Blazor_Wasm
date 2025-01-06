namespace Report_App_WASM.Shared.DTO;

public class DatabaseConnectionDto : BaseTraceabilityDto, IDto
{
    public int DatabaseConnectionId { get; set; }
    [MaxLength(20)] public string ConnectionType { get; set; } = "SQL";
    public TypeDb TypeDb { get; set; }
    [MaxLength(20)] public string? TypeDbName { get; set; }
    [Required][MaxLength(4000)] public string DbConnectionParameters { get; set; } = "[]";
    [MaxLength(1000)] public string? ConnectionLogin { get; set; }
    public string? Password { get; set; }
    public int CommandTimeOut { get; set; } = 300;
    public int CommandFetchSize { get; set; } = 131072;
    public bool UseTablesDescriptions { get; set; } = false;
    public bool UseDescriptionsFromAnotherProvider { get; set; } = false;
    public int IdDescriptions { get; set; }
    public int AdHocQueriesMaxNbrofRowsFetched { get; set; } = 100000;
    public int TaskSchedulerMaxNbrofRowsFetched { get; set; } = 1000000;
    public int DataTransferMaxNbrofRowsFetched { get; set; } = 2000000;
    [MaxLength(4000)] public string MiscParamters { get; set; } = "[]";
    public virtual DataProviderDto? DataProvider { get; set; }
    public virtual ICollection<TableMetadataDto>? TableMetadata { get; set; } =
        new List<TableMetadataDto>();
}