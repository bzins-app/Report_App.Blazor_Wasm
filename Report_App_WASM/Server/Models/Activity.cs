﻿using Report_App_WASM.Server.Models.AuditModels;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Report_App_WASM.Server.Models
{
    public class Activity : BaseTraceability
    {

        public int ActivityId { get; set; }
        [Required]
        [MaxLength(60)]
        public string? ActivityName { get; set; }
        [MaxLength(20)]
        public string? ActivityType { get; set; }
        public bool IsActivated { get; set; }
        public bool Display { get; set; }
        public string? ActivityLogo { get; set; }
        [MaxLength(60)]
        public string? ActivityRoleId { get; set; }
        public virtual ICollection<ActivityDbConnection> ActivityDbConnections { get; set; } = new List<ActivityDbConnection>();
        public virtual ICollection<TaskHeader> TaskHeaders { get; set; } = new List<TaskHeader>();
    }
}
