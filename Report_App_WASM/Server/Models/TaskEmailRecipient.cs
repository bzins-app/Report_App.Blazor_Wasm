using Report_App_WASM.Server.Models.AuditModels;

namespace Report_App_WASM.Server.Models
{
    public class TaskEmailRecipient : BaseTraceability
    {
        public int TaskEmailRecipientId { get; set; }
        public string Email { get; set; } = "[]";
        public string? Message { get; set; }
        public virtual TaskHeader? TaskHeader { get; set; }
    }
}
