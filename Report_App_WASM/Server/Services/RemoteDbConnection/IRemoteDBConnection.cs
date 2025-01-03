namespace Report_App_WASM.Server.Services.RemoteDb;

public interface IRemoteDbConnection
{
    Task<SubmitResult> TestConnectionAsync(ActivityDbConnection parameter);
    Task<DataTable> RemoteDbToDatableAsync(RemoteDbCommandParameters run, CancellationToken cts, int taskId = 0);
    Task<bool> CkeckTableExists(string query, int activityIdTransfer);
    Task CreateTable(string query, int activityIdTransfer);
    Task LoadDatatableToTable(DataTable data, string? targetTable, int activityIdTransfer);
    Task<MergeResult> MergeTables(string query, int activityIdTransfer);
    Task DeleteTable(string tableName, int activityIdTransfer);
    Task<string> GetAllTablesScript(int activityId);
    Task<string> GetTableColumnInfoScript(int activityId, string tableName);
    Task<string> GetAllTablesAndColumnsScript(int activityId);
}