namespace Report_App_WASM.Shared.ApiExchanges;

public class TaksSystemValues
{
    public DateTime Date { get; init; }
    public int NbrWarnings { get; init; }
    public int NbrErrors { get; init; }
    public int NbrCriticals { get; init; }
}