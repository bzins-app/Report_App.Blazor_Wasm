namespace Report_App_WASM.Shared.DTO;

public class TaskEmailRecipientDto : BaseTraceabilityDto, IDto
{
    public int TaskEmailRecipientId { get; set; }
    public string? Email { get; set; }
    public string? Message { get; set; }
    public virtual TaskHeaderDto? TaskHeader { get; set; }
}