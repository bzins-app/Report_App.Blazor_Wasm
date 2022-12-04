using Report_App_WASM.Server.Models.AuditModels;

namespace Report_App_WASM.Server.Models
{
    public class ServicesStatus : BaseTraceability
    {
        public int Id { get; set; }
        public bool EmailService { get; set; }
        public bool ReportService { get; set; }
        public bool AlertService { get; set; }
        public bool DataTransferService { get; set; }
        public bool CleanerService { get; set; }
    }
}
