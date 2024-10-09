namespace Report_App_WASM.Server.Utils.RemoteQueryParameters;

public class RemoteConnectionParameter
{
    public string? ConnnectionString { get; set; }
    public TypeDb TypeDb { get; init; }
    public string? Schema { get; init; }
    public bool UseDbSchema { get; init; }
    public int CommandTimeOut { get; init; } = 300;
    public int CommandFetchSize { get; init; } = 2000;
}