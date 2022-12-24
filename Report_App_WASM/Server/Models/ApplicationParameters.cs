using System.ComponentModel.DataAnnotations;
using Report_App_WASM.Server.Models.AuditModels;

namespace Report_App_WASM.Server.Models
{
    public class ApplicationParameters : BaseTraceability
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string? ApplicationName { get; set; }
        public string? ApplicationLogo { get; set; }
        public string? LoginScreenBackgroundImage { get; set; }
        public string? AdminEmails { get; set; }
        [MaxLength(200)]
        public string? EmailPrefix { get; set; }
        [MaxLength(200)]
        public string? ErrorEmailPrefix { get; set; }
        public string? ErrorEMailMessage { get; set; }
        public string? WelcomeEMailMessage { get; set; }
        [MaxLength(200)]
        public string? AlertEmailPrefix { get; set; }
        public int LogsRetentionInDays { get; set; } = 90;
    }
}
