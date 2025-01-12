namespace Report_App_WASM.Server.Models.AuditModels;

public class BaseTraceability : IBaseEntity
{
    [MaxLength(250)] public string? MiscValue { get; set; }
    public DateTime CreateDateTime { get; set; } = DateTime.Now;
    [MaxLength(100)] public string? CreateUser { get; set; }
    public DateTime ModDateTime { get; set; }
    [MaxLength(100)] public string? ModificationUser { get; set; }
}