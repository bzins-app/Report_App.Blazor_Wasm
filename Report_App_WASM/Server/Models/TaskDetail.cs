using Report_App_WASM.Server.Models.AuditModels;
using System.ComponentModel.DataAnnotations;

namespace Report_App_WASM.Server.Models
{
    public class TaskDetail : BaseTraceability
    {
        public int TaskDetailId { get; set; }
        [Required]
        [MaxLength(100)]
        public string? QueryName { get; set; }
        [Required]
        public string? Query { get; set; }
        public string? TaskDetailParameters { get; set; }
        public string QueryParameters { get; set; } = "[]";
        public int DetailSequence { get; set; } = 99;
        public DateTime? LastRunDateTime { get; set; } = null;
        public int NbrOFCumulativeOccurences { get; set; }
        public virtual TaskHeader? TaskHeader { get; set; }
    }
}
