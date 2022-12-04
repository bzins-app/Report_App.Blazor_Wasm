using Report_App_WASM.Server.Models.AuditModels;
using System;

namespace Report_App_WASM.Server.Models
{
    public class ApplicationLogTaskDetails : IExcludeAuditTrail
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public string? Step { get; set; }
        public string? Info { get; set; }
    }
}
