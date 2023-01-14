using Report_App_WASM.Server.Models.AuditModels;

namespace Report_App_WASM.Server.Models;

public class DbTableDescriptions : BaseTraceability, IExcludeAuditTrail
{
    public int Id { get; set; }
    public string? TableName { get; set; }
    public string? TableDescription { get; set; }
    public string? ColumnName { get; set; }
    public string? ColumnDescription { get; set; }
    public bool IsSnippet { get; set; }
    public virtual ActivityDbConnection? ActivityDbConnection { get; set; }
}