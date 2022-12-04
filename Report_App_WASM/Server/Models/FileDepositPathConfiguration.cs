using Report_App_WASM.Server.Models.AuditModels;
using System.ComponentModel.DataAnnotations;

namespace Report_App_WASM.Server.Models
{
    public class FileDepositPathConfiguration : BaseTraceability
    {
        public int FileDepositPathConfigurationId { get; set; }
        [Required]
        [MaxLength(60)]
        public string? ConfigurationName { get; set; }
        [Required]
        public string FilePath { get; set; } = ".";
        public bool IsReachable { get; set; }
        public bool TryToCreateFolder { get; set; }
        public bool UseSFTPProtocol { get; set; } = false;
        public virtual SFTPConfiguration? SFTPConfiguration { get; set; }
    }
}
