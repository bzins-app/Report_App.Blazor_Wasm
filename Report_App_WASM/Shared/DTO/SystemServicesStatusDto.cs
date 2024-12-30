namespace Report_App_WASM.Shared.DTO;

public class SystemServicesStatusDto : BaseTraceabilityDto, IDto
{
    //public int Id { get; set; }
    //public bool EmailService { get; set; }
    //public bool ReportService { get; set; }
    //public bool AlertService { get; set; }
    //public bool DataTransferService { get; set; }
    //public bool CleanerService { get; set; }

    public int Id { get; set; }
    [MaxLength(200)] public string ServiceName { get; set; }
    public bool IsEnabled { get; set; }
}