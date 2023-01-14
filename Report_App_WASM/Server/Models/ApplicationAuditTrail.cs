using System.ComponentModel.DataAnnotations;
using Report_App_WASM.Server.Models.AuditModels;

namespace Report_App_WASM.Server.Models;

public class ApplicationAuditTrail : IBaseEntity
{
    public int Id { get; set; }

    [MaxLength(100)] public string? UserId { get; set; }

    [MaxLength(20)] public string? Type { get; set; }

    [MaxLength(600)] public string? TableName { get; set; }

    public DateTime DateTime { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? AffectedColumns { get; set; }
    public string? PrimaryKey { get; set; }
}