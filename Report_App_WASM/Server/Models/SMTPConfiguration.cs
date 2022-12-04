using Report_App_WASM.Server.Models.AuditModels;
using Report_App_WASM.Server.Utils.EncryptDecrypt;
using System.ComponentModel.DataAnnotations;

namespace Report_App_WASM.Server.Models
{
    public class SMTPConfiguration : BaseTraceability
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(60)]
        public string? ConfigurationName { get; set; }
        [MaxLength(100)]
        public string? smtpUserName { get; set; }
        private string? _password;
        public string smtpPassword
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
        [Required]
        public string? smtpHost { get; set; }
        [Required]
        public int smtpPort { get; set; }
        public bool smtpSSL { get; set; }
        [Required]
        [MaxLength(100)]
        public string? fromEmail { get; set; }
        [Required]
        [MaxLength(100)]
        public string? fromFullName { get; set; }
        public bool IsActivated { get; set; }
    }
}
