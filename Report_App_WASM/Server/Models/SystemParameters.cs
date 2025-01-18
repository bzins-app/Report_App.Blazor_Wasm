namespace Report_App_WASM.Server.Models;

public class SystemParameters : BaseTraceability
{
    public int Id { get; set; }
    [Required] [MaxLength(400)] public string? ApplicationName { get; set; }
    [MaxLength(1000)] public string? ApplicationLogo { get; set; }
    [MaxLength(1000)] public string? LoginScreenBackgroundImage { get; set; }
    [MaxLength(1000)] public string? AdminEmails { get; set; }
    [MaxLength(200)] public string? EmailPrefix { get; set; }
    [MaxLength(200)] public string? ErrorEmailPrefix { get; set; }
    [MaxLength(4000)] public string? ErrorEMailMessage { get; set; }
    [MaxLength(4000)] public string? WelcomeEMailMessage { get; set; }
    [MaxLength(200)] public string? AlertEmailPrefix { get; set; }
    public int LogsRetentionInDays { get; set; } = 90;
    public bool ActivateTaskSchedulerModule { get; set; }
    public bool ActivateAdHocQueriesModule { get; set; }
}