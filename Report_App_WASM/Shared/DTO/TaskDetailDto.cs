namespace Report_App_WASM.Shared.DTO;

public class TaskDetailDto : BaseTraceabilityDto, IDto
{
    public int TaskDetailId { get; set; }

    [Required] [MaxLength(100)] public string? QueryName { get; set; }

    [Required] public string Query { get; set; } = " ";

    public string? TaskDetailParameters { get; set; }
    public string QueryParameters { get; set; } = "[]";
    public int DetailSequence { get; set; } = 99;
    public DateTime? LastRunDateTime { get; set; } = null;
    public int NbrOfCumulativeOccurences { get; set; }
    public virtual TaskHeaderDto? TaskHeader { get; set; }
}