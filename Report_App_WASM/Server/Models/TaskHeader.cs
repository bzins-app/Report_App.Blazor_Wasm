using Report_App_WASM.Server.Models.AuditModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Report_App_WASM.Server.Models
{
    public class TaskHeader : BaseTraceability
    {
        public TaskHeader()
        {
            TaskDetails = new HashSet<TaskDetail>();
            TaskEmailRecipients = new HashSet<TaskEmailRecipient>();
        }
        public int TaskHeaderId { get; set; }
        [Required]
        [MaxLength(100)]
        public string TaskName { get; set; }
        [Required]
        [MaxLength(60)]
        public string ActivityName { get; set; }
        [MaxLength(60)]
        public string TaskNamePrefix { get; set; }
        [Required]
        [MaxLength(20)]
        public string Type { get; set; }
        [MaxLength(20)]
        public string TypeFile { get; set; }
        public bool IsActivated { get; set; } = false;
        public bool SendByEmail { get; set; } = false;
        public int ReportsRetentionInDays { get; set; } = 90;
        public string Comment { get; set; }
        public string TaskHeaderParameters { get; set; }
        public string CronParameters { get; set; } = "[]";
        public bool UseGlobalQueryParameters { get; set; } = false;
        public string QueryParameters { get; set; } = "[]";
        public DateTime? LastRunDateTime { get; set; } = null;
        public int FileDepositPathConfigurationId { get; set; }
        public virtual ICollection<TaskDetail> TaskDetails { get; set; } = new List<TaskDetail>();
        public virtual ICollection<TaskEmailRecipient> TaskEmailRecipients { get; set; } = new List<TaskEmailRecipient>();
        public virtual Activity Activity { get; set; }
    }
}
