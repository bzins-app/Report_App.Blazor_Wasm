namespace Report_App_WASM.Shared.DTO;

public class FileStorageLocationDto : BaseTraceabilityDto, IDto
{
    [Key] public long FileStorageLocationId { get; set; }
    [Required] [MaxLength(250)] public string? ConfigurationName { get; set; }
    [Required] [MaxLength(4000)] public string FilePath { get; set; } = ".";
    public bool IsReachable { get; set; }
    public bool TryToCreateFolder { get; set; }
    public bool UseSftpProtocol { get; set; } = false;
    public long SftpConfigurationId { get; set; }
}