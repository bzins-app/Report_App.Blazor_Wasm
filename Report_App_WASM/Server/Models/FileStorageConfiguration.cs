namespace Report_App_WASM.Server.Models;

public class FileStorageConfiguration : BaseTraceability
{
    private string? _password;
    public long FileStorageConfigurationId { get; set; }
    public FileStorageConfigurationType ConfigurationType { get; set; }
    [Required] [MaxLength(250)] public string? ConfigurationName { get; set; }
    public string? Host { get; set; }
    public int Port { get; set; } = 22;
    public string? UserName { get; set; }

    public string? Password
    {
        get => _password;
        set
        {
            if (_password == value)
                _password = value;
            else
                _password = EncryptDecrypt.EncryptString(value!);
        }
    }

    public string? ConfigurationParameter { get; set; } = "[]";

    public virtual ICollection<FileStorageLocation>? FileStorageLocations { get; set; }
}