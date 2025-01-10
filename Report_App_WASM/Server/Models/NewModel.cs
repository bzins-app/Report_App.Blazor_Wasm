namespace Report_App_WASM.Server.Models
{

    //[Newtonsoft.Json.JsonConverter(typeof(JsonStringEnumConverter))]
    //public enum ProviderType
    //{
    //    SourceDatabase = 0,
    //    TargetDatabase = 1
    //}
    //public class DataProvider : BaseTraceability
    //{
    //    private string? _providerTypeName;
    //    public int DataProviderId { get; set; }
    //    [Required][MaxLength(250)] public string? ProviderName { get; set; }
    //    public ProviderType ProviderType { get; set; } = ProviderType.SourceDatabase;

    //    [MaxLength(20)]
    //    public string? ProviderTypeName
    //    {
    //        get => _providerTypeName;
    //        set
    //        {
    //            _providerTypeName = value;
    //            _providerTypeName = ProviderType.ToString();
    //        }
    //    }

    //    public bool IsEnabled { get; set; }
    //    public bool IsVisible { get; set; }
    //    [MaxLength(200)] public string? ProviderIcon { get; set; } // Added MaxLength attribute
    //    [MaxLength(100)] public string? ProviderRoleId { get; set; }
    //    [MaxLength(100)] public string TimeZone { get; set; } = TimeZoneInfo.Local.Id;
    //    [MaxLength(1000)] public string MiscParameters { get; set; } = "[]";

    //    public virtual ICollection<DatabaseConnection> DatabaseConnection { get; set; } = new List<DatabaseConnection>();
    //    public virtual ICollection<ScheduledTask> ScheduledTasks { get; set; } = new List<ScheduledTask>();
    //    public virtual ICollection<StoredQuery> StoredQueries { get; set; } = new List<StoredQuery>();
    //}

    //public class DatabaseConnection : BaseTraceability
    //{
    //    private string? _password;
    //    private string? _databaseTypeName;
    //    public int DatabaseConnectionId { get; set; }
    //    [MaxLength(20)] public string ConnectionType { get; set; } = "SQL";
    //    public TypeDb TypeDb { get; set; }

    //    [MaxLength(20)]
    //    public string? TypeDbName
    //    {
    //        get => _databaseTypeName;
    //        set
    //        {
    //            _databaseTypeName = value;
    //            _databaseTypeName = TypeDb.ToString();
    //        }
    //    }

    //    [Required][MaxLength(1000)] public string DbConnectionParameters { get; set; } = "[]";
    //    [MaxLength(200)] public string? ConnectionLogin { get; set; }
    //    [MaxLength(200)]
    //    public string? Password
    //    {
    //        get => _password;
    //        set
    //        {
    //            if (_password == value)
    //                _password = value;
    //            else
    //                _password = EncryptDecrypt.EncryptString(value);
    //        }
    //    }

    //    public int CommandTimeOut { get; set; } = 300;
    //    public int CommandFetchSize { get; set; } = 131072;
    //    public bool UseTablesDescriptions { get; set; } = false;
    //    public bool UseDescriptionsFromAnotherProvider { get; set; } = false;
    //    public int IdDescriptions { get; set; }
    //    public int AdHocQueriesMaxNbrofRowsFetched { get; set; } = 100000;
    //    public int TaskSchedulerMaxNbrofRowsFetched { get; set; } = 1000000;
    //    public int DataTransferMaxNbrofRowsFetched { get; set; } = 2000000;
    //    [MaxLength(1000)] public string MiscParameters { get; set; } = "[]";

    //    public virtual DataProvider? DataProvider { get; set; }
    //    public virtual ICollection<TableMetadata> TableMetadata { get; set; } = new List<TableMetadata>();
    //}

    //public class ScheduledTask : BaseTraceability
    //{
    //    private string? _typeFileName;
    //    private string? _typeName;

    //    public int ScheduledTaskId { get; set; }
    //    [Required][MaxLength(100)] public string? TaskName { get; set; }
    //    [Required][MaxLength(60)] public string ProviderName { get; set; }
    //    public int DataProviderId { get; set; }
    //    [MaxLength(60)] public string? TaskNamePrefix { get; set; }
   // [MaxLength(100)] public string TimeZone { get; set; } = TimeZoneInfo.Local.Id;
    //    public TaskType Type { get; set; }
    //    
    //    [MaxLength(20)]
    //    public string? TypeName
    //    {
    //        get => _typeName;
    //        set
    //        {
    //            _typeName = value;
    //            _typeName = Type.ToString();
    //        }
    //    }

    //    public FileType TypeFile { get; set; }
    //    [MaxLength(20)]
    //    public string? TypeFileName
    //    {
    //        get => _typeFileName;
    //        set
    //        {
    //            _typeFileName = value;
    //            _typeFileName = TypeFile.ToString();
    //        }
    //    }

    //    public bool IsEnabled { get; set; } = false;
    //    public bool SendByEmail { get; set; } = false;
    //    public int ReportsRetentionInDays { get; set; } = 90;
    //    [MaxLength(1000)] public string? Comment { get; set; }
    //    [MaxLength(1000)] public string Tags { get; set; } = "[]";
    //    [MaxLength(1000)] public string TaskHeaderParameters { get; set; } = "[]";
    //    [MaxLength(1000)] public string CronParameters { get; set; } = "[]";
    //    public bool UseGlobalQueryParameters { get; set; } = false;
    //    [MaxLength(1000)] public string QueryParameters { get; set; } = "[]";
    //    public DateTime? LastRunDateTime { get; set; } = null;
    //    public int FileStorageLocationId { get; set; }
    //    [MaxLength(1000)] public string MiscParameters { get; set; } = "[]";

    //    public ICollection<ScheduledTaskQuery> TaskQueries { get; set; } = new List<ScheduledTaskQuery>();
    //    public ICollection<ScheduledTaskDistributionList> DistributionLists { get; set; } = new List<ScheduledTaskDistributionList>();
    //    public DataProvider DataProvider { get; set; } = null!;
    //}

    //public class ScheduledTaskQuery : BaseTraceability
    //{
    //    public int ScheduledTaskQueryId { get; set; }
    //    [Required][MaxLength(100)] public string? QueryName { get; set; }
    //    [Required] public string? Query { get; set; }
    //    [MaxLength(1000)] public string QueryParameters { get; set; } = "[]";
    //    [MaxLength(1000)] public string? ExecutionParameters { get; set; }
    //    public int ExecutionOrder { get; set; } = 99;
    //    public DateTime? LastRunDateTime { get; set; } = null;
    //    public int ExecutionCount { get; set; }
    //    public virtual ScheduledTask? ScheduledTask { get; set; }
    //}

    //public class ScheduledTaskDistributionList : BaseTraceability
    //{
    //    public int ScheduledTaskDistributionListId { get; set; }
    //    [MaxLength(2000)] public string Recipients { get; set; } = "[]";
    //    [MaxLength(3000)] public string? EmailMessage { get; set; }
    //    public virtual ScheduledTask? ScheduledTask { get; set; }
    //}

    //public class StoredQuery : BaseTraceability
    //{
    //    public int Id { get; set; }
    //    public int DataProviderId { get; set; }
    //    [MaxLength(200)] public string? ProviderName { get; set; }
    //    [MaxLength(400)] public string? Comment { get; set; }
    //    [MaxLength(1000)] public string Tags { get; set; } = "[]";
    //    [MaxLength(200)] public string? QueryName { get; set; }
    //    [MaxLength(4000)] public string? Query { get; set; }
    //    [MaxLength(1000)] public string Parameters { get; set; } = "[]";
    //    [MaxLength(1000)] public string QueryParameters { get; set; } = "[]";
    //    [MaxLength(1000)] public string MiscParameters { get; set; } = "[]";
    //    public virtual DataProvider? DataProvider { get; set; }
    //}

    //public class TableMetadata : BaseTraceability, IExcludeAuditTrail
    //{
    //    public int Id { get; set; }
    //    [MaxLength(200)] public string? TableName { get; set; }
    //    [MaxLength(400)] public string? TableDescription { get; set; }
    //    [MaxLength(200)] public string? ColumnName { get; set; }
    //    [MaxLength(400)] public string? ColumnDescription { get; set; }
    //    public bool IsSnippet { get; set; }
    //    [MaxLength(1000)] public string MiscParameters { get; set; } = "[]";
    //    public virtual DatabaseConnection? DatabaseConnection { get; set; }
    //}

    //public class FileStorageLocation : BaseTraceability
    //{
    //    public int FileStorageLocationId { get; set; }
    //    [Required][MaxLength(150)] public string? ConfigurationName { get; set; }
    //    [Required][MaxLength(1000)] public string FilePath { get; set; } = ".";
    //    public bool IsReachable { get; set; }
    //    public bool TryToCreateFolder { get; set; }
    //    public bool UseSftpProtocol { get; set; } = false;
    //    public virtual SftpConfiguration? SftpConfiguration { get; set; }
    //}

    //public class SystemServicesStatus : BaseTraceability
    //{
    //    public int Id { get; set; }
    //    [MaxLength(200)] public string ServiceName { get; set; }
    //    public bool IsEnabled { get; set; }
    //}

    // Logging entities
    //public class SystemLog : Log, IExcludeAuditTrail
    //{
    //    public SystemLog(IHttpContextAccessor accessor)
    //    {
    //        if (accessor.HttpContext != null)
    //        {
    //            Browser = accessor.HttpContext.Request.Headers["Sec-CH-UA"];
    //            Platform = accessor.HttpContext.Request.Headers["User-Agent"];
    //            FullVersion = accessor.HttpContext.Request.Headers["Sec-CH-UA-Full-Version"];
    //            User = accessor.HttpContext.User?.Identity?.Name;
    //            Path = accessor.HttpContext.Request.Path;
    //            Host = accessor.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
    //            if (string.IsNullOrEmpty(Host)) Host = accessor.HttpContext.Connection?.RemoteIpAddress?.ToString();
    //        }
    //    }

    //    protected SystemLog() { }

    //    public DateTime TimeStampAppHour { get; set; } = DateTime.Now;
    //    [MaxLength(600)] public string? Browser { get; set; }
    //    [MaxLength(600)] public string? Platform { get; set; }
    //    [MaxLength(600)] public string? FullVersion { get; set; }
    //    [MaxLength(600)] public string? Host { get; set; }
    //    [MaxLength(600)] public string? Path { get; set; }
    //    [MaxLength(200)] public string? User { get; set; }
    //}

    //public class TaskLog : IExcludeAuditTrail
    //{
    //    public int TaskLogId { get; set; }
    //    public int TaskId { get; set; }
    //    public DateTime StartDateTime { get; set; }
    //    public DateTime EndDateTime { get; set; }
    //    public int DurationInSeconds { get; set; }
    //    public int DataProviderId { get; set; }
    //    [MaxLength(200)] public string? ProviderName { get; set; }
    //    [MaxLength(200)] public string? JobDescription { get; set; }
    //    [MaxLength(60)] public string? Type { get; set; }
    //    [MaxLength(1000)] public string? Result { get; set; }
    //    public bool Error { get; set; }
    //    [MaxLength(200)] public string? RunBy { get; set; }
    //}

    //public class TaskStepLog : IExcludeAuditTrail
    //{
    //    public int Id { get; set; }
    //    public int TaskLogId { get; set; }
    //    public int TaskId { get; set; }
    //    public DateTime TimeStamp { get; set; } = DateTime.Now;
    //    [MaxLength(1000)] public string? Step { get; set; }
    //    [MaxLength(1000)] public string? Info { get; set; }
    //}

    //public class AdHocQueryExecutionLog : IExcludeAuditTrail
    //{
    //    public int Id { get; set; }
    //    public int QueryId { get; set; }
    //    public DateTime StartDateTime { get; set; } = DateTime.Now;
    //    public DateTime EndDateTime { get; set; }
    //    public int DurationInSeconds { get; set; }
    //    public int DataProviderId { get; set; }
    //    [MaxLength(60)] public string? ProviderName { get; set; }
    //    [MaxLength(60)] public string? JobDescription { get; set; }
    //    [MaxLength(60)] public string? Type { get; set; }
    //    public int NbrOfRows { get; set; }
    //    [MaxLength(1000)] public string? Result { get; set; }
    //    public bool Error { get; set; }
    //    [MaxLength(1000)] public string? RunBy { get; set; }
    //}

    //public class QueryExecutionLog : IExcludeAuditTrail
    //{
    //    public int Id { get; set; }
    //    [MaxLength(1000)] public string? TypeDb { get; set; }
    //    [MaxLength(1000)] public string? Database { get; set; }
    //    public int CommandTimeOut { get; set; }
    //    public int DataProviderId { get; set; }
    //    [MaxLength(60)] public string? ProviderName { get; set; }
    //    [MaxLength(1000)] public string? QueryName { get; set; }
    //    [MaxLength(4000)] public string? Query { get; set; }
    //    public DateTime StartDateTime { get; set; }
    //    public DateTime TransferBeginDateTime { get; set; }
    //    public DateTime EndDateTime { get; set; }
    //    public TimeSpan TotalDuration { get; set; }
    //    public TimeSpan SqlExcecutionDuration { get; set; }
    //    public TimeSpan DownloadDuration { get; set; }
    //    public int NbrOfRows { get; set; }
    //    [MaxLength(1000)] public string? RunBy { get; set; }
    //    [MaxLength(1000)] public string? TypeJob { get; set; }
    //}

    // Continuation of EmailLog
    //public class EmailLog : IExcludeAuditTrail
    //{
    //    public int Id { get; set; }
    //    public DateTime StartDateTime { get; set; }
    //    public DateTime EndDateTime { get; set; }
    //    public int DurationInSeconds { get; set; }
    //    [MaxLength(1000)] public string? EmailTitle { get; set; }
    //    [MaxLength(1000)] public string? Result { get; set; }
    //    public bool Error { get; set; }
    //    public int NbrOfRecipients { get; set; }
    //    [MaxLength(2000)] public string? RecipientList { get; set; }
    //}

    //public class ReportGenerationLog : IExcludeAuditTrail
    //{
    //    public int Id { get; set; }
    //    public DateTime CreatedAt { get; set; }
    //    [MaxLength(200)] public string? CreatedBy { get; set; }
    //    public int DataProviderId { get; set; }
    //    [MaxLength(200)] public string? ProviderName { get; set; }
    //    public int ScheduledTaskId { get; set; }
    //    [MaxLength(200)] public string? ReportName { get; set; }
    //    [MaxLength(200)] public string? SubName { get; set; }
    //    [MaxLength(60)] public string? FileType { get; set; }
    //    [MaxLength(100)] public string? FileName { get; set; }
    //    [MaxLength(200)] public string? ReportPath { get; set; }
    //    public double FileSizeInMb { get; set; }
    //    public bool IsAvailable { get; set; } = true;
    //    [MaxLength(200)] public string? Result { get; set; }
    //    public bool Error { get; set; }
    //}

    //public class AuditTrail : IBaseEntity
    //{
    //    public int Id { get; set; }
    //    [MaxLength(100)] public string? UserId { get; set; }
    //    [MaxLength(20)] public string? Type { get; set; }
    //    [MaxLength(600)] public string? TableName { get; set; }
    //    public DateTime DateTime { get; set; }
    //    [MaxLength(1000)] public string? OldValues { get; set; }
    //    [MaxLength(1000)] public string? NewValues { get; set; }
    //    [MaxLength(1000)] public string? AffectedColumns { get; set; }
    //    [MaxLength(200)] public string? PrimaryKey { get; set; }
    //}

    //public class SystemParameters : BaseTraceability
    //{
    //    public int Id { get; set; }
    //    [Required][MaxLength(400)] public string? ApplicationName { get; init; }
    //    [MaxLength(1000)] public string? ApplicationLogo { get; init; }
    //    [MaxLength(1000)] public string? LoginScreenBackgroundImage { get; set; }
    //    [MaxLength(1000)] public string? AdminEmails { get; init; }
    //    [MaxLength(200)] public string? EmailPrefix { get; init; }
    //    [MaxLength(200)] public string? ErrorEmailPrefix { get; init; }
    //    [MaxLength(1000)] public string? ErrorEMailMessage { get; init; }
    //    [MaxLength(1000)] public string? WelcomeEMailMessage { get; set; }
    //    [MaxLength(200)] public string? AlertEmailPrefix { get; init; }
    //    public int LogsRetentionInDays { get; set; } = 90;
    //    public bool ActivateTaskSchedulerModule { get; set; }
    //    public bool ActivateAdHocQueriesModule { get; set; }
    //}

    //public class SystemUniqueKey : IExcludeAuditTrail
    //{
    //    public Guid Id { get; set; }
    //}

    //public enum AuditType
    //{
    //    None = 0,
    //    Create = 1,
    //    Update = 2,
    //    Delete = 3
    //}

    //public class AuditEntry
    //{
    //    public AuditEntry(EntityEntry entry)
    //    {
    //        Entry = entry;
    //    }

    //    public EntityEntry Entry { get; }
    //    public string? UserId { get; init; } = null!;
    //    public string TableName { get; init; } = null!;
    //    public Dictionary<string, object?> KeyValues { get; } = new();
    //    public Dictionary<string, object?> OldValues { get; } = new();
    //    public Dictionary<string, object?> NewValues { get; } = new();
    //    public AuditType AuditType { get; set; }
    //    public List<string> ChangedColumns { get; } = new();

    //    public AuditTrail ToAudit()
    //    {
    //        var audit = new AuditTrail
    //        {
    //            UserId = UserId,
    //            Type = AuditType.ToString(),
    //            TableName = TableName,
    //            DateTime = DateTime.Now,
    //            PrimaryKey = JsonConvert.SerializeObject(KeyValues),
    //            OldValues = OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues),
    //            NewValues = NewValues.Count == 0 ? null : JsonConvert.SerializeObject(NewValues),
    //            AffectedColumns = ChangedColumns.Count == 0 ? null : JsonConvert.SerializeObject(ChangedColumns)
    //        };
    //        return audit;
    //    }
    //}

    //public class UserPreferences : BaseTraceability
    //{
    //    public int Id { get; set; }
    //    [MaxLength(100)]public string? UserId { get; set; }
    //    [MaxLength(200)]public string? SaveName { get; set; }
    //    [MaxLength(1000)]public string? Parameters { get; set; }
    //    public TypeConfiguration TypeConfiguration { get; set; }
    //    public int IdIntConfiguration { get; set; }
    //    [MaxLength(100)]public string? IdStringConfiguration { get; set; }
    //    [MaxLength(1000)] public string? SavedValues { get; set; }
    //    [MaxLength(1000)] public string MiscParameters { get; set; } = "[]";
    //}
}
