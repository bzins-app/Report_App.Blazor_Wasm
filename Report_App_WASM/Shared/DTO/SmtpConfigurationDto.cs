namespace Report_App_WASM.Shared.DTO;

public class SmtpConfigurationDto : BaseTraceabilityDto, IDto
{
    public int Id { get; set; }
    [Required] [MaxLength(250)] public string? ConfigurationName { get; set; }
    [MaxLength(200)] public string? SmtpUserName { get; set; }
    public string? SmtpPassword { get; set; }
    [Required] public string? SmtpHost { get; set; }
    [Required] public int SmtpPort { get; set; }
    public bool SmtpSsl { get; set; }
    [Required] [MaxLength(250)] public string? FromEmail { get; set; }
    [Required] [MaxLength(250)] public string? FromFullName { get; set; }
    public bool IsActivated { get; set; }
}