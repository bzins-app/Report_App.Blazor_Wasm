using Report_App_WASM.Server.Models.AuditModels;

namespace Report_App_WASM.Server.Models;

public class QueryStore : BaseTraceability
{
    public int Id { get; set; }
    public int IdActivity { get; set; }
    public string? ActivityName { get; set; }
    public string? Comment { get; set; }
    public string Tags { get; set; } = "[]";
    public string? QueryName { get; set; }
    public string? Query { get; set; }
    public string Parameters { get; set; } = "[]";
    public string QueryParameters { get; set; } = "[]";
    public virtual Activity? Activity { get; set; }
}