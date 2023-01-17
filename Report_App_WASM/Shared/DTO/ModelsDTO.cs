using System.ComponentModel.DataAnnotations;

namespace Report_App_WASM.Shared.DTO;

public class ApplicationUserDto : BaseTraceabilityDto, IDto
{
    public virtual Guid Id { get; set; } = default!;
    public virtual string? UserName { get; set; }
    public virtual string? Email { get; set; }
    public virtual bool EmailConfirmed { get; set; }
    public virtual string? PasswordHash { get; set; }
    public virtual string? PhoneNumber { get; set; }
    public virtual bool PhoneNumberConfirmed { get; set; }
    public virtual bool TwoFactorEnabled { get; set; }
    public virtual DateTimeOffset? LockoutEnd { get; set; }
    public virtual bool LockoutEnabled { get; set; }

    public virtual int AccessFailedCount { get; set; }

    //override identity user, add new column
    public bool IsBaseUser { get; set; } = false;

    [MaxLength(100)] public string? UserFirstName { get; set; }

    [MaxLength(100)] public string? UserLastName { get; set; }

    [MaxLength(10)] public string? ApplicationTheme { get; set; }

    [MaxLength(5)] public string Culture { get; set; } = "en";
}

public class ApplicationParametersDto : BaseTraceabilityDto, IDto
{
    public int Id { get; set; }

    [Required] [MaxLength(100)] public string? ApplicationName { get; set; }

    public string? ApplicationLogo { get; set; }
    public string? LoginScreenBackgroundImage { get; set; }
    public string? AdminEmails { get; set; }

    [MaxLength(200)] public string? EmailPrefix { get; set; }

    [MaxLength(200)] public string? ErrorEmailPrefix { get; set; }

    public string? ErrorEMailMessage { get; set; }
    public string? WelcomeEMailMessage { get; set; }

    [MaxLength(200)] public string? AlertEmailPrefix { get; set; }

    public int LogsRetentionInDays { get; set; } = 90;
    public bool ActivateTaskSchedulerModule { get; set; }
    public bool ActivateAdHocQueriesModule { get; set; }
}

public class SftpConfigurationDto : BaseTraceabilityDto, IDto
{
    public int SftpConfigurationId { get; set; }
    public bool UseFtpProtocol { get; set; }

    [Required] [MaxLength(60)] public string? ConfigurationName { get; set; }

    public string? Host { get; set; }
    public int Port { get; set; } = 22;
    public string? UserName { get; set; }
    public string? Password { get; set; }
}

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

public class QueryStoreDto : BaseTraceabilityDto, IDto
{
    public int Id { get; set; }
    public int IdActivity { get; set; }
    public string? ActivityName { get; set; }
    public string? Comment { get; set; }
    public string Tags { get; set; } = "[]";
    public string? QueryName { get; set; }
    public string? Query { get; set; }
    public string Parameters { get; set; } = "[]";
    public string QueryParameters { get; set; } = "[]";
    public virtual ActivityDto? Activity { get; set; }
}

public class ActivityDbConnectionDto : BaseTraceabilityDto, IDto
{
    public int Id { get; set; }

    [MaxLength(20)] public string ConnectionType { get; set; } = "SQL";

    public TypeDb TypeDb { get; set; }

    [MaxLength(20)] public string? TypeDbName { get; set; }

    [Required] public string? ConnectionPath { get; set; }

    public int Port { get; set; }

    [MaxLength(100)] public string? ConnectionLogin { get; set; }

    public string? Password { get; set; }
    public bool UseDbSchema { get; set; }
    public bool AdAuthentication { get; set; } = false;
    public bool IntentReadOnly { get; set; }

    [MaxLength(100)] public string? DbSchema { get; set; }

    public int CommandTimeOut { get; set; } = 300;
    public int CommandFetchSize { get; set; } = 131072;
    public string DbConnectionParameters { get; set; } = "[]";
    public bool UseTablesDescriptions { get; set; } = false;
    public bool UseDescriptionsFromAnotherActivity { get; set; } = false;
    public int IdDescriptions { get; set; }
    public int AdHocQueriesMaxNbrofRowsFetched { get; set; } = 100000;
    public int TaskSchedulerMaxNbrofRowsFetched { get; set; } = 1000000;
    public int DataTransferMaxNbrofRowsFetched { get; set; } = 2000000;
    public virtual ActivityDto? Activity { get; set; }

    public virtual ICollection<DbTableDescriptionsDto>? DbTableDescriptions { get; set; } =
        new List<DbTableDescriptionsDto>();
}

public class DbTableDescriptionsDto : BaseTraceabilityDto, IDto
{
    public int Id { get; set; }
    public string? TableName { get; set; }
    public string? TableDescription { get; set; }
    public string? ColumnName { get; set; }
    public string? ColumnDescription { get; set; }
    public bool IsSnippet { get; set; }
    public virtual ActivityDbConnectionDto? ActivityDbConnection { get; set; }
}

public class FileDepositPathConfigurationDto : BaseTraceabilityDto, IDto
{
    [Key] public int FileDepositPathConfigurationId { get; set; }

    [Required] [MaxLength(60)] public string? ConfigurationName { get; set; }

    [Required] public string FilePath { get; set; } = ".";

    public bool IsReachable { get; set; }
    public bool TryToCreateFolder { get; set; }
    public bool UseSftpProtocol { get; set; } = false;
    public int SftpConfigurationId { get; set; }
}

public class ServicesStatusDto : BaseTraceabilityDto, IDto
{
    public int Id { get; set; }
    public bool EmailService { get; set; }
    public bool ReportService { get; set; }
    public bool AlertService { get; set; }
    public bool DataTransferService { get; set; }
    public bool CleanerService { get; set; }
}

public class SmtpConfigurationDto : BaseTraceabilityDto, IDto
{
    public int Id { get; set; }

    [Required] [MaxLength(60)] public string? ConfigurationName { get; set; }

    [MaxLength(100)] public string? SmtpUserName { get; set; }

    public string? SmtpPassword { get; set; }

    [Required] public string? SmtpHost { get; set; }

    [Required] public int SmtpPort { get; set; }

    public bool SmtpSsl { get; set; }

    [Required] [MaxLength(100)] public string? FromEmail { get; set; }

    [Required] [MaxLength(100)] public string? FromFullName { get; set; }

    public bool IsActivated { get; set; }
}

public class LdapConfigurationDto : BaseTraceabilityDto, IDto
{
    public int Id { get; set; }

    [Required] [MaxLength(60)] public string? ConfigurationName { get; set; }

    [Required] public string? Domain { get; set; }

    [MaxLength(100)] public string? UserName { get; set; }

    public string? Password { get; set; }
    public bool IsActivated { get; set; }
}

public class TaskDetailDto : BaseTraceabilityDto, IDto
{
    public int TaskDetailId { get; set; }

    [Required] [MaxLength(100)] public string? QueryName { get; set; }

    [Required] public string Query { get; set; } = " ";

    public string? TaskDetailParameters { get; set; }
    public string QueryParameters { get; set; } = "[]";
    public int DetailSequence { get; set; } = 99;
    public DateTime? LastRunDateTime { get; set; } = null;
    public int NbrOfCumulativeOccurences { get; set; }
    public virtual TaskHeaderDto? TaskHeader { get; set; }
}

public class TaskEmailRecipientDto : BaseTraceabilityDto, IDto
{
    public int TaskEmailRecipientId { get; set; }
    public string? Email { get; set; }
    public string? Message { get; set; }
    public virtual TaskHeaderDto? TaskHeader { get; set; }
}

public class UserSavedConfigurationDto : BaseTraceabilityDto, IDto
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? SaveName { get; set; }
    public string? Parameters { get; set; }
    public TypeConfiguration TypeConfiguration { get; set; }
    public int IdIntConfiguration { get; set; }
    public string? IdStringConfiguration { get; set; }
    public string? SavedValues { get; set; }
}

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

public class ApplicationLogQueryExecutionDto : IDto
{
    public int Id { get; set; }
    public string? TypeDb { get; set; }
    public string? Database { get; set; }
    public int CommandTimeOut { get; set; }
    public int ActivityId { get; set; }

    [MaxLength(60)] public string? ActivityName { get; set; }

    public string? QueryName { get; set; }
    public string? Query { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime TransferBeginDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string? TotalDuration { get; set; }
    public string? SqlExcecutionDuration { get; set; }
    public string? DownloadDuration { get; set; }
    public int NbrOfRows { get; set; }
    public string? RunBy { get; set; }
    public string? TypeJob { get; set; }
}

public class ApplicationLogTaskDto : IDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int DurationInSeconds { get; set; }
    public int ActivityId { get; set; }

    [MaxLength(60)] public string? ActivityName { get; set; }

    [MaxLength(60)] public string? JobDescription { get; set; }

    [MaxLength(60)] public string? Type { get; set; }

    public string? Result { get; set; }
    public bool Error { get; set; }
    public string? RunBy { get; set; }
}

public class ApplicationLogTaskDetailsDto : IDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.Now;
    public string? Step { get; set; }
    public string? Info { get; set; }
}

public class ApplicationLogEmailSenderDto : IDto
{
    public int Id { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int DurationInSeconds { get; set; }
    public string? EmailTitle { get; set; }
    public string? Result { get; set; }
    public bool Error { get; set; }
    public int NbrOfRecipients { get; set; }
    public string? RecipientList { get; set; }
}

public class ApplicationAuditTrailDto : IDto
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? Type { get; set; }
    public string? TableName { get; set; }
    public DateTime DateTime { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? AffectedColumns { get; set; }
    public string? PrimaryKey { get; set; }
}

public class ApplicationLogSystemDto : IDto
{
    public int Id { get; set; }
    public DateTime TimeStampAppHour { get; set; } = DateTime.Now;

    [MaxLength(600)] public string? Browser { get; set; }

    [MaxLength(600)] public string? Platform { get; set; }

    [MaxLength(600)] public string? FullVersion { get; set; }

    [MaxLength(600)] public string? Host { get; set; }

    [MaxLength(600)] public string? Path { get; set; }

    [MaxLength(200)] public string? User { get; set; }

    public int EventId { get; set; }
    public int Level { get; set; }
    public string? Message { get; set; }
    public string? Name { get; set; }
}

public class ApplicationLogReportResultDto : IDto
{
    private double _fileSizeInMb;
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }

    [MaxLength(100)] public string? CreatedBy { get; set; }

    public int ActivityId { get; set; }

    [MaxLength(60)] public string? ActivityName { get; set; }

    public int TaskHeaderId { get; set; }

    [MaxLength(200)] public string? ReportName { get; set; }

    [MaxLength(200)] public string? SubName { get; set; }

    [MaxLength(60)] public string? FileType { get; set; }

    [MaxLength(100)] public string? FileName { get; set; }

    public string? ReportPath { get; set; }

    public double FileSizeInMb
    {
        get => _fileSizeInMb;
        set => _fileSizeInMb = Math.Round(value, 2);
    }

    public bool IsAvailable { get; set; } = true;
    public string? Result { get; set; }
    public bool Error { get; set; }
}

public class ApplicationLogAdHocQueriesDto : IDto
{
    public int Id { get; set; }
    public int QueryId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int DurationInSeconds { get; set; }
    public int ActivityId { get; set; }

    [MaxLength(60)] public string? ActivityName { get; set; }

    [MaxLength(60)] public string? JobDescription { get; set; }

    [MaxLength(60)] public string? Type { get; set; }

    public int NbrOfRows { get; set; }
    public string? Result { get; set; }
    public bool Error { get; set; }
    public string? RunBy { get; set; }
}