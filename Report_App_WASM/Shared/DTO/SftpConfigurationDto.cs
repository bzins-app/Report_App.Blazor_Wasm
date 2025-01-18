namespace Report_App_WASM.Shared.DTO;

public class SftpConfigurationDto : BaseTraceabilityDto, IDto
{
    public int SftpConfigurationId { get; set; }
    public bool UseFtpProtocol { get; set; }
    [Required] [MaxLength(250)] public string? ConfigurationName { get; set; }
    public string? Host { get; set; }
    public int Port { get; set; } = 22;
    public string? UserName { get; set; }
    public string? Password { get; set; }
}