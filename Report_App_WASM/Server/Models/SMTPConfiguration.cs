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
        public string? SmtpUserName { get; set; }
        private string? _password;
        public string? SmtpPassword
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
                    _password = EncryptDecrypt.EncryptString(value!);
                }
            }
        }
        [Required]
        public string? SmtpHost { get; set; }
        [Required]
        public int SmtpPort { get; set; }
        public bool SmtpSSL { get; set; }
        [Required]
        [MaxLength(100)]
        public string? FromEmail { get; set; }
        [Required]
        [MaxLength(100)]
        public string? FromFullName { get; set; }
        public bool IsActivated { get; set; }
    }
}
