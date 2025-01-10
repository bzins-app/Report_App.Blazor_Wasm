namespace Report_App_WASM.Server.Models;

public class AuditTrail : IBaseEntity
{
    public int Id { get; set; }
    [MaxLength(250)] public string? UserId { get; set; }
    [MaxLength(250)] public string? Type { get; set; }
    [MaxLength(600)] public string? TableName { get; set; }
    public DateTime DateTime { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? AffectedColumns { get; set; }
    [MaxLength(1000)] public string? PrimaryKey { get; set; }
}