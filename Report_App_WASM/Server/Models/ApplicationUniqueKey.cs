using Report_App_WASM.Server.Models.AuditModels;
using System;

namespace Report_App_WASM.Server.Models
{
    public class ApplicationUniqueKey : IExcludeAuditTrail
    {
        public Guid Id { get; set; }
    }
}
