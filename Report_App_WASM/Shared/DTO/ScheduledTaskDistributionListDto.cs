namespace Report_App_WASM.Shared.DTO;

public class ScheduledTaskDistributionListDto : BaseTraceabilityDto, IDto
{
    public int ScheduledTaskDistributionListId { get; set; }
    [MaxLength(4000)] public string Recipients { get; set; } = "[]";
    [MaxLength(4000)] public string? EmailMessage { get; set; }
    public virtual ScheduledTaskDto? ScheduledTask { get; set; }
}