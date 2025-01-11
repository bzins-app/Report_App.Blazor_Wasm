namespace Report_App_WASM.Shared.DTO;

public class AuditTrailDto : IDto
{
    public long Id { get; set; }
    [MaxLength(250)] public string? UserId { get; set; }
    [MaxLength(250)] public string? Type { get; set; }
    [MaxLength(600)] public string? TableName { get; set; }
    public DateTime DateTime { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? AffectedColumns { get; set; }
    [MaxLength(1000)] public string? PrimaryKey { get; set; }
}