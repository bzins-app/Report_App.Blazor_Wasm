namespace Report_App_WASM.Server.Models;

public class EmailLog : IExcludeAuditTrail
{
    public int Id { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int DurationInSeconds { get; set; }
    [MaxLength(1000)] public string? EmailTitle { get; set; }
    [MaxLength(1000)] public string? Result { get; set; }
    public bool Error { get; set; }
    public int NbrOfRecipients { get; set; }
    [MaxLength(2000)] public string? RecipientList { get; set; }
}