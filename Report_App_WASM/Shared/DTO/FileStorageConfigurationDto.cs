namespace Report_App_WASM.Shared.DTO;

public class FileStorageConfigurationDto : BaseTraceabilityDto, IDto
{
    public long FileStorageConfigurationId { get; set; }
    public FileStorageConfigurationType ConfigurationType { get; set; }
    [Required] [MaxLength(250)] public string? ConfigurationName { get; set; }
    public string? Host { get; set; }
    public int Port { get; set; } = 22;
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? ConfigurationParameter { get; set; }
}