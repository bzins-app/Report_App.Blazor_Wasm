namespace Report_App_WASM.Server.Utils.RemoteDb.RemoteQueryParameters;

public class RemoteConnectionParameter
{
    public string? ConnnectionString { get; set; }
    public TypeDb TypeDb { get; set; }
    public int CommandTimeOut { get; set; } = 300;
    public int CommandFetchSize { get; set; } = 2000;
    public bool UseDbSchema { get; set; }
    public string? Schema { get; set; }
}