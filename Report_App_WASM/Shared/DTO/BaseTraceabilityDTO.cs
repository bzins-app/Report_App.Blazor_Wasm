namespace Report_App_WASM.Shared.DTO;

public class BaseTraceabilityDto
{
    [MaxLength(250)] public string? MiscValue { get; set; }
    public DateTime CreateDateTime { get; set; } = DateTime.Now;
    [MaxLength(100)] public string? CreateUser { get; set; }
    public DateTime ModDateTime { get; set; }
    [MaxLength(100)] public string? ModificationUser { get; set; }
}