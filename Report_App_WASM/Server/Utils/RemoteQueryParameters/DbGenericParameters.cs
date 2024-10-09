namespace Report_App_WASM.Server.Utils.RemoteQueryParameters;

public class DbGenericParameters
{
    public List<string> IntializationQueries = new();
    public DbConnection? DbConnection { get; set; }
    public DbCommand? DbCommand { get; set; }
    public DbDataAdapter? DbDataAdapter { get; set; }
}