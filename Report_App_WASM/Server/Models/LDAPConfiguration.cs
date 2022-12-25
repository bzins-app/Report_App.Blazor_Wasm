using Report_App_WASM.Server.Models.AuditModels;
using Report_App_WASM.Server.Utils.EncryptDecrypt;
using System.ComponentModel.DataAnnotations;

namespace Report_App_WASM.Server.Models
{
    public class LdapConfiguration : BaseTraceability
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(60)]
        public string? ConfigurationName { get; set; }
        [Required]
        public string? Domain { get; set; }
        [MaxLength(100)]
        public string? UserName { get; set; }
        private string? _password;
        public string? Password
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
        public bool IsActivated { get; set; }
    }
}
