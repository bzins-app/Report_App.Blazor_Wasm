using Report_App_WASM.Server.Models.AuditModels;
using Report_App_WASM.Shared;
using System.ComponentModel.DataAnnotations;

namespace Report_App_WASM.Server.Models
{
    public class Activity : BaseTraceability
    {

        public int ActivityId { get; set; }
        [Required]
        [MaxLength(60)]
        public string? ActivityName { get; set; }
        public ActivityType ActivityType { get; set; } = ActivityType.SourceDb;
        private string? _activityTypeName;

        [MaxLength(20)]
        public string? ActivityTypeName
        {
            get => _activityTypeName; set
            {
                _activityTypeName = value;
                _activityTypeName = ActivityType.ToString();
            }
        }
        public bool IsActivated { get; set; }
        public bool Display { get; set; }
        public string? ActivityLogo { get; set; }
        [MaxLength(60)]
        public string? ActivityRoleId { get; set; }
        public virtual ICollection<ActivityDbConnection> ActivityDbConnections { get; set; } = new List<ActivityDbConnection>();
        public virtual ICollection<TaskHeader> TaskHeaders { get; set; } = new List<TaskHeader>();
    }
}
