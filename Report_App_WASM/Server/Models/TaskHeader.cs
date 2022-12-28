using Report_App_WASM.Server.Models.AuditModels;
using Report_App_WASM.Shared;
using System.ComponentModel.DataAnnotations;

namespace Report_App_WASM.Server.Models
{
    public sealed class TaskHeader : BaseTraceability
    {
        public TaskHeader()
        {
            TaskDetails = new HashSet<TaskDetail>();
            TaskEmailRecipients = new HashSet<TaskEmailRecipient>();
        }
        public int TaskHeaderId { get; set; }
        [Required]
        [MaxLength(100)]
        public string? TaskName { get; set; } = null!;

        [Required]
        [MaxLength(60)]
        public string ActivityName { get; set; } = null!;

        public int IdActivity { get; set; }
        [MaxLength(60)]
        public string? TaskNamePrefix { get; set; }
        public TaskType Type { get; set; }
        private string? _typeName;
        [MaxLength(20)]
        public string? TypeName
        {
            get => _typeName; set
            {
                _typeName = value;
                _typeName = Type.ToString();
            }
        }
        public FileType TypeFile { get; set; }
        private string? _typeFileName;
        [MaxLength(20)]
        public string? TypeFileName
        {
            get => _typeFileName; set
            {
                _typeName = value;
                _typeFileName = TypeFile.ToString();
            }
        }
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
        public ICollection<TaskDetail> TaskDetails { get; set; } = new List<TaskDetail>();
        public ICollection<TaskEmailRecipient> TaskEmailRecipients { get; set; } = new List<TaskEmailRecipient>();
        public Activity Activity { get; set; } = null!;
    }
}
