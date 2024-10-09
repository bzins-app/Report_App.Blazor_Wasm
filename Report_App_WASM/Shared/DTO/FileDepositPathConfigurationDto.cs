namespace Report_App_WASM.Shared.DTO;

public class FileDepositPathConfigurationDto : BaseTraceabilityDto, IDto
{
    [Key] public int FileDepositPathConfigurationId { get; set; }

    [Required] [MaxLength(60)] public string? ConfigurationName { get; set; }

    [Required] public string FilePath { get; set; } = ".";

    public bool IsReachable { get; set; }
    public bool TryToCreateFolder { get; set; }
    public bool UseSftpProtocol { get; set; } = false;
    public int SftpConfigurationId { get; set; }
}