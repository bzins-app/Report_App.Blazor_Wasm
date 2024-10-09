namespace Report_App_WASM.Shared.DTO;

public class ApplicationParametersDto : BaseTraceabilityDto, IDto
{
    public int Id { get; set; }

    [Required] [MaxLength(100)] public string? ApplicationName { get; set; }

    public string? ApplicationLogo { get; set; }
    public string? LoginScreenBackgroundImage { get; set; }
    public string? AdminEmails { get; set; }

    [MaxLength(200)] public string? EmailPrefix { get; set; }

    [MaxLength(200)] public string? ErrorEmailPrefix { get; set; }

    public string? ErrorEMailMessage { get; set; }
    public string? WelcomeEMailMessage { get; set; }

    [MaxLength(200)] public string? AlertEmailPrefix { get; set; }

    public int LogsRetentionInDays { get; set; } = 90;
    public bool ActivateTaskSchedulerModule { get; set; }
    public bool ActivateAdHocQueriesModule { get; set; }
}