namespace Report_App_WASM.Server.Utils.RemoteDb;

public interface IRemoteDb : IDisposable
{
    string GetAllTablesScript(DatabaseConnection dbInfo);
    string GetAllTablesAndColumnsScript(DatabaseConnection dbInfo);
    string GetTableColumnInfoScript(DatabaseConnection dbInfo, string tableName);
    Task TryConnectAsync(DatabaseConnection dbInfo);
    Task TryConnectAsync(string ConnnectionString);

    Task<DataTable> RemoteDbToDatableAsync(DataTable data, RemoteDbCommandParameters run,
        DatabaseConnection dbInfo, CancellationToken cts);
}