namespace Report_App_WASM.Server.Models;

public class SystemServicesStatus : BaseTraceability
{
    public int Id { get; set; }
    public bool EmailService { get; set; }
    public bool ReportService { get; set; }
    public bool AlertService { get; set; }
    public bool DataTransferService { get; set; }
    public bool CleanerService { get; set; }
}

//public class SystemServicesStatus : BaseTraceability
//{
//    public int Id { get; set; }
//    [MaxLength(200)] public string ServiceName { get; set; }
//    public bool IsEnabled { get; set; }
//}