using Report_App_WASM.Server.Models.AuditModels;
using System.ComponentModel.DataAnnotations;

namespace Report_App_WASM.Server.Models
{
    public class ApplicationLogAdHocQueries : IExcludeAuditTrail
    {
        public int Id { get; set; }
        public int QueryId { get; set; }
        public DateTime StartDateTime { get; set; }=DateTime.Now;
        public DateTime EndDateTime { get; set; }
        public int DurationInSeconds { get; set; }
        public int ActivityId { get; set; }

        [MaxLength(60)] public string? ActivityName { get; set; }

        [MaxLength(60)] public string? JobDescription { get; set; }

        [MaxLength(60)] public string? Type { get; set; }

        public int NbrOfRows { get; set; }
        public string? Result { get; set; }
        public bool Error { get; set; }
        public string? RunBy { get; set; }
    }
}
