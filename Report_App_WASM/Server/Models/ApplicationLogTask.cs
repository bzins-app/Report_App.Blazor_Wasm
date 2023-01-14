using System.ComponentModel.DataAnnotations;
using Report_App_WASM.Server.Models.AuditModels;

namespace Report_App_WASM.Server.Models;

public class ApplicationLogTask : IExcludeAuditTrail
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int DurationInSeconds { get; set; }
    public int ActivityId { get; set; }

    [MaxLength(60)] public string? ActivityName { get; set; }

    [MaxLength(60)] public string? JobDescription { get; set; }

    [MaxLength(60)] public string? Type { get; set; }

    public string? Result { get; set; }
    public bool Error { get; set; }
    public string? RunBy { get; set; }
}