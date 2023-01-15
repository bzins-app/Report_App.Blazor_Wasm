namespace Report_App_WASM.Shared.ExternalApi;

public class TriggTask
{
    public int TaksId { get; set; }
    public bool? GenerateFileToFolder { get; set; }
    public bool? SendEmail { get; set; }
}