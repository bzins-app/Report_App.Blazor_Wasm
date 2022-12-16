using Report_App_WASM.Server.Models.AuditModels;

namespace Report_App_WASM.Server.Models
{
    public class ApplicationUniqueKey : IExcludeAuditTrail
    {
        public Guid Id { get; set; }
    }
}
