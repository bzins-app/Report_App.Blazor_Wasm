using System.ComponentModel.DataAnnotations;

namespace Report_App_WASM.Server.Models.AuditModels
{
    public class BaseTraceability : IBaseEntity
    {
        public DateTime CreateDateTime { get; set; } = DateTime.Now;
        [MaxLength(100)]
        public string? CreateUser { get; set; }
        public DateTime ModDateTime { get; set; }
        [MaxLength(100)]
        public string? ModificationUser { get; set; }
    }
}
