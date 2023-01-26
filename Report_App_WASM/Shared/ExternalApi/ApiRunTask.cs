namespace Report_App_WASM.Shared.ExternalApi;

public class ApiRunTask
{
    public int TaksId { get; set; }
    public bool GenerateFileToFolder { get; set; }
    public bool SendEmail { get; set; }
    public bool Test { get; set; }
}