namespace Report_App_WASM.Server.Models;

public class ApplicationParameters : BaseTraceability
{
    public int Id { get; set; }

    [Required] [MaxLength(100)] public string? ApplicationName { get; init; }

    public string? ApplicationLogo { get; init; }
    public string? LoginScreenBackgroundImage { get; set; }
    public string? AdminEmails { get; init; }

    [MaxLength(200)] public string? EmailPrefix { get; init; }

    [MaxLength(200)] public string? ErrorEmailPrefix { get; init; }

    public string? ErrorEMailMessage { get; init; }
    public string? WelcomeEMailMessage { get; set; }

    [MaxLength(200)] public string? AlertEmailPrefix { get; init; }

    public int LogsRetentionInDays { get; set; } = 90;
    public bool ActivateTaskSchedulerModule { get; set; }
    public bool ActivateAdHocQueriesModule { get; set; }
}