using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace Report_App_WASM.Server.Models.AuditModels
{
    public class AuditEntry
    {
        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }
        public EntityEntry Entry { get; }
        public string UserId { get; set; } = null!;
        public string TableName { get; set; } = null!;
        public Dictionary<string, object> KeyValues { get; } = new();
        public Dictionary<string, object> OldValues { get; } = new();
        public Dictionary<string, object> NewValues { get; } = new();
        public AuditType AuditType { get; set; }
        public List<string> ChangedColumns { get; } = new();
        public ApplicationAuditTrail ToAudit()
        {
            var audit = new ApplicationAuditTrail
            {
                UserId = UserId,
                Type = AuditType.ToString(),
                TableName = TableName,
                DateTime = DateTime.Now,
                PrimaryKey = JsonConvert.SerializeObject(KeyValues),
                OldValues = OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues),
                NewValues = NewValues.Count == 0 ? null : JsonConvert.SerializeObject(NewValues),
                AffectedColumns = ChangedColumns.Count == 0 ? null : JsonConvert.SerializeObject(ChangedColumns)
            };
            return audit;
        }
    }
}
