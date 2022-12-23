using System.ComponentModel.DataAnnotations;

namespace Report_App_WASM.Shared.DTO
{
    public class ApplicationParametersDTO : BaseTraceabilityDTO, IDTO
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string? ApplicationName { get; set; }
        public string? ApplicationLogo { get; set; }
        public string? LoginScreenBackgroundImage { get; set; }
        public string? AdminEmails { get; set; }
        [MaxLength(200)]
        public string? EmailPrefix { get; set; }
        [MaxLength(200)]
        public string? ErrorEmailPrefix { get; set; }
        public string? ErrorEMailMessage { get; set; }
        public string? WelcomeEMailMessage { get; set; }
        [MaxLength(200)]
        public string? AlertEmailPrefix { get; set; }
        public int LogsRetentionInDays { get; set; } = 90;
    }
    public class SFTPConfigurationDTO : BaseTraceabilityDTO, IDTO
    {
        public int SFTPConfigurationId { get; set; }
        public bool UseFTPProtocol { get; set; }
        [Required]
        [MaxLength(60)]
        public string? ConfigurationName { get; set; }
        public string? Host { get; set; }
        public int Port { get; set; } = 22;
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
    public class ActivityDTO : BaseTraceabilityDTO, IDTO
    {

        public int ActivityId { get; set; }
        [Required]
        [MaxLength(60)]
        public string? ActivityName { get; set; }
        public ActivityType ActivityType { get; set; } = ActivityType.SourceDB;
        [MaxLength(20)]
        public string? ActivityTypeName { get; set; }
        public bool IsActivated { get; set; }
        public bool Display { get; set; }
        public string? ActivityLogo { get; set; }
        [MaxLength(60)]
        public string? ActivityRoleId { get; set; }
        public virtual ICollection<ActivityDbConnectionDTO> ActivityDbConnections { get; set; } = new List<ActivityDbConnectionDTO>();
        public virtual ICollection<TaskHeaderDTO> TaskHeaders { get; set; } = new List<TaskHeaderDTO>();
    }

    public class ActivityDbConnectionDTO : BaseTraceabilityDTO, IDTO
    {
        public int Id { get; set; }
        [MaxLength(20)]
        public string ConnectionType { get; set; } = "SQL";
        public TypeDb TypeDb { get; set; }
        [MaxLength(20)]
        public string? TypeDbName { get; set; }
        [Required]
        public string? ConnectionPath { get; set; }
        public int Port { get; set; }
        [MaxLength(100)]
        public string? ConnectionLogin { get; set; }
        public string? Password { get; set; }
        public bool UseDbSchema { get; set; }
        public bool ADAuthentication { get; set; } = false;
        public bool IntentReadOnly { get; set; }
        [MaxLength(100)]
        public string? DbSchema { get; set; }
        public int CommandTimeOut { get; set; } = 300;
        public int CommandFetchSize { get; set; } = 131072;
        public string DbConnectionParameters { get; set; } = "[]";
        public virtual ActivityDTO? Activity { get; set; }
    }

    public class FileDepositPathConfigurationDTO : BaseTraceabilityDTO, IDTO
    {
        public int FileDepositPathConfigurationId { get; set; }
        [Required]
        [MaxLength(60)]
        public string? ConfigurationName { get; set; }
        [Required]
        public string FilePath { get; set; } = ".";
        public bool IsReachable { get; set; }
        public bool TryToCreateFolder { get; set; }
        public bool UseSFTPProtocol { get; set; } = false;
        public virtual SFTPConfigurationDTO? SFTPConfiguration { get; set; }
    }

    public class ServicesStatusDTO : BaseTraceabilityDTO, IDTO
    {
        public int Id { get; set; }
        public bool EmailService { get; set; }
        public bool ReportService { get; set; }
        public bool AlertService { get; set; }
        public bool DataTransferService { get; set; }
        public bool CleanerService { get; set; }

    }

    public class SMTPConfigurationDTO : BaseTraceabilityDTO, IDTO
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(60)]
        public string? ConfigurationName { get; set; }
        [MaxLength(100)]
        public string? SmtpUserName { get; set; }
        public string? SmtpPassword { get; set; }
        [Required]
        public string? SmtpHost { get; set; }
        [Required]
        public int SmtpPort { get; set; }
        public bool SmtpSSL { get; set; }
        [Required]
        [MaxLength(100)]
        public string? FromEmail { get; set; }
        [Required]
        [MaxLength(100)]
        public string? FromFullName { get; set; }
        public bool IsActivated { get; set; }
    }

    public class LDAPConfigurationDTO : BaseTraceabilityDTO, IDTO
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(60)]
        public string? ConfigurationName { get; set; }
        [Required]
        public string? Domain { get; set; }
        [MaxLength(100)]
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public bool IsActivated { get; set; }
    }

    public class TaskDetailDTO : BaseTraceabilityDTO, IDTO
    {
        public int TaskDetailId { get; set; }
        [Required]
        [MaxLength(100)]
        public string? QueryName { get; set; }
        [Required]
        public string Query { get; set; } = " ";
        public string? TaskDetailParameters { get; set; }
        public string QueryParameters { get; set; } = "[]";
        public int DetailSequence { get; set; } = 99;
        public DateTime? LastRunDateTime { get; set; } = null;
        public int NbrOFCumulativeOccurences { get; set; }
        public virtual TaskHeaderDTO? TaskHeader { get; set; }
    }

    public class TaskEmailRecipientDTO : BaseTraceabilityDTO, IDTO
    {
        public int TaskEmailRecipientId { get; set; }
        public string? Email { get; set; }
        public string? Message { get; set; }
        public virtual TaskHeaderDTO? TaskHeader { get; set; }
    }

    public class TaskHeaderDTO : BaseTraceabilityDTO, IDTO
    {
        public TaskHeaderDTO()
        {
            TaskDetails = new HashSet<TaskDetailDTO>();
            TaskEmailRecipients = new HashSet<TaskEmailRecipientDTO>();
        }
        public int TaskHeaderId { get; set; }
        [Required]
        [MaxLength(100)]
        public string TaskName { get; set; }
        [Required]
        [MaxLength(60)]
        public string ActivityName { get; set; }
        public int IdActivity { get; set; }
        [MaxLength(60)]
        public string? TaskNamePrefix { get; set; }
        public TaskType Type { get; set; }
        [MaxLength(20)]
        public string? TypeName { get; set; }
        public FileType TypeFile { get; set; }
        [MaxLength(20)]
        public string? TypeFileName { get; set; }
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
        public virtual ICollection<TaskDetailDTO> TaskDetails { get; set; } = new List<TaskDetailDTO>();
        public virtual ICollection<TaskEmailRecipientDTO> TaskEmailRecipients { get; set; } = new List<TaskEmailRecipientDTO>();
        public virtual ActivityDTO Activity { get; set; }
    }

    public class ApplicationLogQueryExecutionDTO : IDTO
    {
        public int Id { get; set; }
        public string? TypeDb { get; set; }
        public string? Database { get; set; }
        public int CommandTimeOut { get; set; }
        public int ActivityId { get; set; }
        [MaxLength(60)]
        public string? ActivityName { get; set; }
        public string? QueryName { get; set; }
        public string? Query { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime TransferBeginDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public TimeSpan? TotalDuration { get; set; }
        public TimeSpan? SQLExcecutionDuration { get; set; }
        public TimeSpan? DownloadDuration { get; set; }
        public int NbrOfRows { get; set; }
        public string? RunBy { get; set; }
        public string? TypeJob { get; set; }
    }

    public class ApplicationLogTaskDTO : IDTO
    {
        public int Id { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int DurationInSeconds { get; set; }
        public int ActivityId { get; set; }
        [MaxLength(60)]
        public string? ActivityName { get; set; }
        [MaxLength(60)]
        public string? JobDescription { get; set; }
        [MaxLength(60)]
        public string? Type { get; set; }
        public string? Result { get; set; }
        public bool Error { get; set; }
        public string? RunBy { get; set; }
    }

    public class ApplicationLogTaskDetailsDTO : IDTO
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public string? Step { get; set; }
        public string? Info { get; set; }
    }

    public class ApplicationLogEmailSenderDTO : IDTO
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

    public class ApplicationAuditTrailDTO : IDTO
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

    public class ApplicationLogSystemDTO : IDTO
    {
        public int Id { get; set; }
        public DateTime TimeStampAppHour { get; set; } = DateTime.Now;
        [MaxLength(600)]
        public string? Browser { get; set; }
        [MaxLength(600)]
        public string? Platform { get; set; }
        [MaxLength(600)]
        public string? FullVersion { get; set; }
        [MaxLength(600)]
        public string? Host { get; set; }
        [MaxLength(600)]
        public string? Path { get; set; }
        [MaxLength(200)]
        public string? User { get; set; }
        public int EventId { get; set; }
        public int Level { get; set; }
        public string? Message { get; set; }
        public string? Name { get; set; }
    }

    public class ApplicationLogReportResultDTO : IDTO
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
        public int ActivityId { get; set; }
        [MaxLength(60)]
        public string? ActivityName { get; set; }
        public int TaskHeaderId { get; set; }
        [MaxLength(200)]
        public string? ReportName { get; set; }
        [MaxLength(200)]
        public string? SubName { get; set; }
        [MaxLength(60)]
        public string? FileType { get; set; }
        [MaxLength(100)]
        public string? FileName { get; set; }
        public string? ReportPath { get; set; }
        private double _FileSizeInMB;
        public double FileSizeInMB
        {
            get { return _FileSizeInMB; }
            set { _FileSizeInMB = Math.Round(value, 2); }
        }
        public bool IsAvailable { get; set; } = true;
        public string? Result { get; set; }
        public bool Error { get; set; }
    }
}
