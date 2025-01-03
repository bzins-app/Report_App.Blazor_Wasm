using Report_App_WASM.Server.Utils.RemoteDb;

namespace Report_App_WASM.Server.Services.RemoteDb;

public class RemoteDbConnection : IRemoteDbConnection, IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private DateTime _fillTimeStamp;

    private bool _first = true;

    public RemoteDbConnection(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public async Task<bool> CkeckTableExists(string query, int activityIdTransfer)
    {
        var activityId = await GetDataTransferActivity(activityIdTransfer);
        var _dbInfo = await GetDbInfo(activityId);
        SqlServerRemoteDb remote = new();
        return await remote.CkeckTableExists(_dbInfo, query);
    }

    public async Task DeleteTable(string tableName, int activityIdTransfer)
    {
        var activityId = await GetDataTransferActivity(activityIdTransfer);
        var _dbInfo = await GetDbInfo(activityId);
        SqlServerRemoteDb remote = new();
        await remote.DeleteTable(_dbInfo, tableName);
    }

    public async Task CreateTable(string query, int activityIdTransfer)
    {
        var activityId = await GetDataTransferActivity(activityIdTransfer);
        var _dbInfo = await GetDbInfo(activityId);
        SqlServerRemoteDb remote = new();
        await remote.CreateTable(_dbInfo, query);
    }

    public async Task<MergeResult> MergeTables(string query, int activityIdTransfer)
    {
        var activityId = await GetDataTransferActivity(activityIdTransfer);
        var _dbInfo = await GetDbInfo(activityId);
        SqlServerRemoteDb remote = new();
        return await remote.MergeTables(_dbInfo, query);
    }

    public async Task LoadDatatableToTable(DataTable data, string? targetTable, int activityIdTransfer)
    {
        var activityId = await GetDataTransferActivity(activityIdTransfer);
        var _dbInfo = await GetDbInfo(activityId);
        SqlServerRemoteDb remote = new();
        await remote.LoadDatatableToTable(_dbInfo, data, targetTable);
    }

    public async Task<string> GetAllTablesScript(int activityId)
    {
        var _dbInfo = await GetDbInfo(activityId);
        var remote = GetRemoteDbType(_dbInfo.TypeDb);
        return remote.GetAllTablesScript(_dbInfo);
    }


    public async Task<string> GetAllTablesAndColumnsScript(int activityId)
    {
        var _dbInfo = await GetDbInfo(activityId);
        var remote = GetRemoteDbType(_dbInfo.TypeDb);
        return remote.GetAllTablesAndColumnsScript(_dbInfo);
    }

    public async Task<string> GetTableColumnInfoScript(int activityId, string tableName)
    {
        var _dbInfo = await GetDbInfo(activityId);
        var remote = GetRemoteDbType(_dbInfo.TypeDb);
        return remote.GetTableColumnInfoScript(_dbInfo, tableName);
    }

    public async Task<SubmitResult> TestConnectionAsync(ActivityDbConnection parameter)
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
            var activityName = await _context.Activity.Where(a => a.ActivityId == run.ActivityId)
                .Select(a => a.ActivityName).FirstOrDefaultAsync(cts);
            var _dbInfo = await GetDbInfo(run.ActivityId);
            var remote = GetRemoteDbType(_dbInfo.TypeDb);
            var logTask = new ApplicationLogTaskDetails { TaskId = taskId, Step = "Fetch data", Info = run.QueryInfo };
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
                    ApplicationLogQueryExecution logQuery = new()
                    {
                        ActivityId = run.ActivityId,
                        Database = _dbInfo.DbSchema,
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
                        NbrOfRows = values.Rows.Count,
                        ActivityName = activityName
                    };
                    await _context.AddAsync(logQuery);
                    await _context.SaveChangesAsync();
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
                    logTask.Info +=
                        $" Exception caught on attempt {attempts} - will retry after delay in {delay / 1000} seconds " +
                        ex.Message;
                    logTask.Step += $": attempt {attempts}";
                    await _context.AddAsync(logTask);
                    await _context.SaveChangesAsync();
                    await Task.Delay(delay, cts).WaitAsync(cts);
                }
            }
        } while (true);
    }

    private async Task<int> GetDataTransferActivity(int activityId)
    {
        return await _context.Activity.Where(a => a.ActivityType == ActivityType.TargetDb && a.ActivityId == activityId)
            .Select(a => a.ActivityId)
            .FirstOrDefaultAsync();
    }

    private IRemoteDb GetRemoteDbType(TypeDb typeDb)
    {
        return typeDb switch
        {
            TypeDb.Oracle => new OracleRemoteDb(),
            TypeDb.SqlServer => new SqlServerRemoteDb(),
            TypeDb.Db2 => new OlebDbDbRemoteDb(),
            TypeDb.OlebDb => new OlebDbDbRemoteDb(),
            TypeDb.MariaDb => new MariaDbRemoteDb(),
            TypeDb.PostgreSql => new PostgreSqlRemoteDb(),
            _ => new MySqlDbRemoteDb()
        };
    }

    private async Task<ActivityDbConnection> GetDbInfo(int activityId)
    {
        return (await _context.ActivityDbConnection.Where(a => a.Activity.ActivityId == activityId)
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