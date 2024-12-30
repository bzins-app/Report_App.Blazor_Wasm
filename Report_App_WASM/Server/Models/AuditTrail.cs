namespace Report_App_WASM.Server.Models;

public class AuditTrail : IBaseEntity
{
    public int Id { get; set; }
    [MaxLength(100)] public string? UserId { get; set; }
    [MaxLength(20)] public string? Type { get; set; }
    [MaxLength(600)] public string? TableName { get; set; }
    public DateTime DateTime { get; set; }
    [MaxLength(1000)] public string? OldValues { get; set; }
    [MaxLength(1000)] public string? NewValues { get; set; }
    [MaxLength(1000)] public string? AffectedColumns { get; set; }
    [MaxLength(200)] public string? PrimaryKey { get; set; }
}