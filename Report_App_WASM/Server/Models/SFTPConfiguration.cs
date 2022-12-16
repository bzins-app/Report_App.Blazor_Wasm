using Report_App_WASM.Server.Models.AuditModels;
using Report_App_WASM.Server.Utils.EncryptDecrypt;
using System.ComponentModel.DataAnnotations;

namespace Report_App_WASM.Server.Models
{
    public class SFTPConfiguration : BaseTraceability
    {
        public int SFTPConfigurationId { get; set; }
        public bool UseFTPProtocol { get; set; }
        [Required]
        [MaxLength(60)]
        public string? ConfigurationName { get; set; }
        public string? Host { get; set; }
        public int Port { get; set; } = 22;
        public string? UserName { get; set; }
        private string? _password;
        public string Password
        {
            get => _password;
            set
            {
                if (_password == value)
                {
                    _password = value;
                }
                else
                {
                    _password = EncryptDecrypt.EncryptString(value);
                }
            }
        }
        public virtual ICollection<FileDepositPathConfiguration>? FileDepositPathConfigurations { get; set; }
    }
}
