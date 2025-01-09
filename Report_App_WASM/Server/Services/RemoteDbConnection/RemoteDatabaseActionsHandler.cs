using Report_App_WASM.Server.Utils.RemoteDb;
using Report_App_WASM.Shared.DatabasesConnectionParameters;

namespace Report_App_WASM.Server.Services.RemoteDb;

public class RemoteDatabaseActionsHandler : IRemoteDatabaseActionsHandler, IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private DateTime _fillTimeStamp;

    private bool _first = true;

    public RemoteDatabaseActionsHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public async Task<bool> CkeckTableExists(string query, int dataProviderIdTransfer)
    {
        var dataProviderId = await GetDataTransferActivity(dataProviderIdTransfer);
        var _dbInfo = await GetDbInfo(dataProviderId);
        SqlServerRemoteDb remote = new();
        return await remote.CkeckTableExists(_dbInfo, query);
    }

    public async Task DeleteTable(string tableName, int dataProviderIdTransfer)
    {
        var dataProviderId = await GetDataTransferActivity(dataProviderIdTransfer);
        var _dbInfo = await GetDbInfo(dataProviderId);
        SqlServerRemoteDb remote = new();
        await remote.DeleteTable(_dbInfo, tableName);
    }

    public async Task CreateTable(string query, int dataProviderIdTransfer)
    {
        var dataProviderId = await GetDataTransferActivity(dataProviderIdTransfer);
        var _dbInfo = await GetDbInfo(dataProviderId);
        SqlServerRemoteDb remote = new();
        await remote.CreateTable(_dbInfo, query);
    }

    public async Task<MergeResult> MergeTables(string query, int dataProviderIdTransfer)
    {
        var dataProviderId = await GetDataTransferActivity(dataProviderIdTransfer);
        var _dbInfo = await GetDbInfo(dataProviderId);
        SqlServerRemoteDb remote = new();
        return await remote.MergeTables(_dbInfo, query);
    }

    public async Task LoadDatatableToTable(DataTable data, string? targetTable, int dataProviderIdTransfer)
    {
        var dataProviderId = await GetDataTransferActivity(dataProviderIdTransfer);
        var _dbInfo = await GetDbInfo(dataProviderId);
        SqlServerRemoteDb remote = new();
        await remote.LoadDatatableToTable(_dbInfo, data, targetTable);
    }

    public async Task<string> GetAllTablesScript(int dataProviderId)
    {
        var _dbInfo = await GetDbInfo(dataProviderId);
        var remote = GetRemoteDbType(_dbInfo.TypeDb);
        return remote.GetAllTablesScript(_dbInfo);
    }


    public async Task<string> GetAllTablesAndColumnsScript(int dataProviderId)
    {
        var _dbInfo = await GetDbInfo(dataProviderId);
        var remote = GetRemoteDbType(_dbInfo.TypeDb);
        return remote.GetAllTablesAndColumnsScript(_dbInfo);
    }

    public async Task<string> GetTableColumnInfoScript(int dataProviderId, string tableName)
    {
        var _dbInfo = await GetDbInfo(dataProviderId);
        var remote = GetRemoteDbType(_dbInfo.TypeDb);
        return remote.GetTableColumnInfoScript(_dbInfo, tableName);
    }

    public async Task<SubmitResult> TestConnectionAsync(DatabaseConnection parameter)
    {
        try
        {
            var remote = GetRemoteDbType(parameter.TypeDb);
            await remote.TryConnectAsync(parameter);
            return new SubmitResult { Success = true, Message = "OK" };
        }
        catch (Exception e)
        {
            return new SubmitResult { Success = false, Message = e.Message };
        }
    }

    public async Task<DataTable> RemoteDbToDatableAsync(RemoteDbCommandParameters run, CancellationToken cts,
        int taskId = 0)
    {
        DataTable values;
        var attempts = 0;
        if (run.Test) attempts = 3;
        do
        {
            var activityName = await _context.DataProvider.Where(a => a.DataProviderId == run.DataProviderId)
                .Select(a => a.ProviderName).FirstOrDefaultAsync(cts);
            var _dbInfo = await GetDbInfo(run.DataProviderId);
            var dbparam=DatabaseConnectionParametersManager.DeserializeFromJson(_dbInfo.DbConnectionParameters, "", "");
            var remote = GetRemoteDbType(_dbInfo.TypeDb);
            var _logTaskStep = new TaskStepLog { TaskLogId = taskId, Step = "Fetch data", Info = run.QueryInfo };
            try
            {
                attempts++;
                DataTable dataTable = new();
                dataTable.RowChanged += OnInitialized;
                var start = DateTime.Now;
                _first = true;
                _fillTimeStamp = DateTime.Now;


                values = await remote.RemoteDbToDatableAsync(dataTable, run, _dbInfo, cts);
                dataTable.RowChanged -= OnInitialized;

                DateTime fill;
                if (values.Rows.Count == 0)
                    fill = DateTime.Now;
                else
                    fill = _fillTimeStamp;
                var end = DateTime.Now;
                if (!run.Test)
                {
                    QueryExecutionLog logQuery = new()
                    {
                        DataProviderId = run.DataProviderId,
                        Database = dbparam.Database,
                        TypeDb = _dbInfo.TypeDb.ToString(),
                        CommandTimeOut = _dbInfo.CommandTimeOut,
                        StartDateTime = start,
                        TotalDuration = end - start,
                        SqlExcecutionDuration = fill - start,
                        DownloadDuration = end - fill,
                        EndDateTime = end,
                        TransferBeginDateTime = fill,
                        QueryName = run.QueryInfo,
                        Query = run.QueryToRun,
                        RowsFetched = values.Rows.Count,
                        ProviderName = activityName,
                        TaskLogId = taskId,
                        ScheduledTaskQueryId = run.ScheduledTaskQueryId,
                        ScheduledTaskId = run.ScheduledTaskId
                    };
                    await _context.AddAsync(logQuery, cts);
                    await _context.SaveChangesAsync("backgroundworker");
                    var _qstep = new TaskStepLog
                    {
                        TaskLogId = taskId,
                        Step = "Fetch data completed",
                        Info = run.QueryInfo + "- Nbr of rows:" + values.Rows.Count,
                        RelatedLogType = LogType.QueryExecutionLog, RelatedLogId = logQuery.Id
                    };
                    await _context.AddAsync(_qstep, cts);
                    await _context.SaveChangesAsync("backgroundworker");
                }

                remote.Dispose();
                return values;
                //break; // Sucess! Lets exit the loop!
            }
            catch (Exception ex)
            {
                if (attempts == 4)
                    throw;

                if (cts.IsCancellationRequested)
                    throw;

                var delay = attempts switch
                {
                    1 => 10 * 1000,
                    2 => 60 * 1000,
                    3 => 10 * 60 * 1000,
                    _ => 10 * 1000
                };
                if (!run.Test)
                {
                    _logTaskStep.Info +=
                        $" Exception caught on attempt {attempts} - will retry after delay in {delay / 1000} seconds " +
                        ex.Message;
                    _logTaskStep.Step += $": attempt {attempts}";
                    await _context.AddAsync(_logTaskStep, cts);
                    await _context.SaveChangesAsync();
                    await Task.Delay(delay, cts).WaitAsync(cts);
                }
            }
        } while (true);
    }

    private async Task<int> GetDataTransferActivity(int dataProviderId)
    {
        return await _context.DataProvider.Where(a => a.ProviderType == ProviderType.TargetDatabase && a.DataProviderId == dataProviderId)
            .Select(( a) => a.DataProviderId)
            .FirstOrDefaultAsync();
    }

    private IRemoteDb GetRemoteDbType(TypeDb typeDb)
    {
        return typeDb switch
        {
            TypeDb.Oracle => new OracleRemoteDb(),
            TypeDb.SqlServer => new SqlServerRemoteDb(),
            TypeDb.OlebDb => new OlebDbDbRemoteDb(),
            TypeDb.MariaDb => new MariaDbRemoteDb(),
            TypeDb.PostgreSql => new PostgreSqlRemoteDb(),
            _ => new MySqlDbRemoteDb()
        };
    }

    private async Task<DatabaseConnection> GetDbInfo(int dataProviderId)
    {
        return (await _context.DatabaseConnection.Where(a => a.DataProvider.DataProviderId == dataProviderId)
            .FirstOrDefaultAsync())!;
    }

    private void OnInitialized(object sender, DataRowChangeEventArgs args)
    {
        if (_first)
        {
            _fillTimeStamp = DateTime.Now;
            _first = false;
        }
    }
}