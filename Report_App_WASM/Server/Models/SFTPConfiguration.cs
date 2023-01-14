using System.ComponentModel.DataAnnotations;
using Report_App_WASM.Server.Models.AuditModels;
using Report_App_WASM.Server.Utils.EncryptDecrypt;

namespace Report_App_WASM.Server.Models;

public class SftpConfiguration : BaseTraceability
{
    private string? _password;
    public int SftpConfigurationId { get; set; }
    public bool UseFtpProtocol { get; set; }

    [Required] [MaxLength(60)] public string? ConfigurationName { get; set; }

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

    public virtual ICollection<FileDepositPathConfiguration>? FileDepositPathConfigurations { get; set; }
}