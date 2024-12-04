namespace Report_App_WASM.Shared.DTO;

public class ApplicationLogTaskDetailsDto : IDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public DateTime TimeStamp { get; set; } = DateTime.Now;
    public string? Step { get; set; }
    public string? Info { get; set; }
}