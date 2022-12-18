using Report_App_WASM.Server.Models.AuditModels;

namespace Report_App_WASM.Server.Models
{
    public class QueryStore : BaseTraceability
    {
        public int Id { get; set; }
        public string? TypeDb { get; set; }
        public string? Typoplogy { get; set; }
        public string? Area { get; set; }
        public string? QueryName { get; set; }
        public string? Query { get; set; }
        public string QueryParameters { get; set; } = "[]";
        public int NbrOfRuns { get; set; }
        public DateTime LastRun { get; set; }
    }
}
