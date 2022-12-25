using Report_App_WASM.Server.Models.AuditModels;
using System.ComponentModel.DataAnnotations;

namespace Report_App_WASM.Server.Models
{
    public class ApplicationLogQueryExecution : IExcludeAuditTrail
    {
        public int Id { get; set; }
        public string? TypeDb { get; set; }
        public string? Database { get; set; }
        public int CommandTimeOut { get; set; }
        public int ActivityId { get; set; }
        [MaxLength(60)]
        public string? ActivityName { get; set; }
        public string? QueryName { get; set; }
        public string? Query { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime TransferBeginDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public TimeSpan SqlExcecutionDuration { get; set; }
        public TimeSpan DownloadDuration { get; set; }
        public int NbrOfRows { get; set; }
        public string? RunBy { get; set; }
        public string? TypeJob { get; set; }
    }
}
