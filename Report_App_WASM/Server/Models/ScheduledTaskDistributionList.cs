namespace Report_App_WASM.Server.Models;

public class ScheduledTaskDistributionList : BaseTraceability
{
    public long ScheduledTaskDistributionListId { get; set; }
    [MaxLength(4000)] public string Recipients { get; set; } = "[]";
    [MaxLength(4000)] public string? EmailMessage { get; set; }
    public virtual ScheduledTask? ScheduledTask { get; set; }
}