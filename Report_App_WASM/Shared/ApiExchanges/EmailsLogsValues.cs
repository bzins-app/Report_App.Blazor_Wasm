namespace Report_App_WASM.Shared.ApiExchanges;

public class EmailsLogsValues
{
    public DateTime Date { get; init; }
    public int NbrEmails { get; init; }
    public int NbrErrors { get; init; }
    public int TotalDuration { get; init; }
}