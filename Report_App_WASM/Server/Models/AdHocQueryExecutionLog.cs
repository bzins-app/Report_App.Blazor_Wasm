namespace Report_App_WASM.Server.Models;

public class AdHocQueryExecutionLog : IExcludeAuditTrail
{
    public int Id { get; set; }
    public int QueryId { get; set; }
    public DateTime StartDateTime { get; set; } = DateTime.Now;
    public DateTime EndDateTime { get; set; }
    public int DurationInSeconds { get; set; }
    public int DataProviderId { get; set; }
    [MaxLength(250)] public string? ProviderName { get; set; }
    [MaxLength(250)] public string? JobDescription { get; set; }
    [MaxLength(60)] public string? Type { get; set; }
    public int NbrOfRows { get; set; }
    [MaxLength(4000)] public string? Result { get; set; }
    public bool Error { get; set; }
    [MaxLength(1000)] public string? RunBy { get; set; }
}