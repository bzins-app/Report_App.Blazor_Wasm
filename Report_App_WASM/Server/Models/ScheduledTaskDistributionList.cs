namespace Report_App_WASM.Server.Models;

public class ScheduledTaskDistributionList : BaseTraceability
{
    public int ScheduledTaskDistributionListId { get; set; }
    [MaxLength(2000)] public string Recipients { get; set; } = "[]";
    [MaxLength(3000)] public string? EmailMessage { get; set; }
    public virtual ScheduledTask? ScheduledTask { get; set; }
}