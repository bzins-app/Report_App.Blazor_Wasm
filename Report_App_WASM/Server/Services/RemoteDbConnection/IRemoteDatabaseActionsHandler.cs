namespace Report_App_WASM.Server.Services.RemoteDb;

public interface IRemoteDatabaseActionsHandler
{
    Task<SubmitResult> TestConnectionAsync(DatabaseConnection parameter);
    Task<DataTable> RemoteDbToDatableAsync(RemoteDbCommandParameters run, CancellationToken cts, long taskId = 0);
    Task<bool> CkeckTableExists(string query, long activityIdTransfer);
    Task CreateTable(string query, long activityIdTransfer);
    Task LoadDatatableToTable(DataTable data, string? targetTable, long activityIdTransfer);
    Task<MergeResult> MergeTables(string query, long activityIdTransfer);
    Task DeleteTable(string tableName, long activityIdTransfer);
    Task<string> GetAllTablesScript(long activityId);
    Task<string> GetTableColumnInfoScript(long activityId, string tableName);
    Task<string> GetAllTablesAndColumnsScript(long activityId);
}