namespace Report_App_WASM.Shared.DTO;

public class TaskLogDto : IDto
{
    public int TaskLogId { get; set; }
    public int TaskId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int DurationInSeconds { get; set; }
    public int DataProviderId { get; set; }
    [MaxLength(250)] public string? ProviderName { get; set; }
    [MaxLength(250)] public string? JobDescription { get; set; }
    [MaxLength(60)] public string? Type { get; set; }
    public string? Result { get; set; }
    public bool Error { get; set; }
    [MaxLength(200)] public string? RunBy { get; set; }
}