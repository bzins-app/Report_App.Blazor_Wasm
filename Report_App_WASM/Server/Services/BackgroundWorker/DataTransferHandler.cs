using System.Text.Json;
using Report_App_WASM.Server.Services.EmailSender;
using Report_App_WASM.Server.Services.FilesManagement;
using Report_App_WASM.Server.Services.RemoteDb;
using Report_App_WASM.Server.Utils.BackgroundWorker;

namespace Report_App_WASM.Server.Services.BackgroundWorker
{
    public class DataTransferHandler : ScheduledTaskHandler
    {

        public DataTransferHandler(ApplicationDbContext context, IEmailSender emailSender,
            IRemoteDatabaseActionsHandler dbReader, LocalFilesService fileDeposit, IMapper mapper,
            IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _emailSender = emailSender;
            _dbReader = dbReader;
            _fileDeposit = fileDeposit;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
        }

        private class DataTransferRowsStats
        {
            public int BulkInserted { get; set; }
            public int Inserted { get; set; }
            public int Updated { get; set; }
            public int Deleted { get; set; }
        }

        DataTransferRowsStats _dataTransferStat = new DataTransferRowsStats();


        public async ValueTask HandleDatatransferTask(TaskJobParameters parameters)
        {
            _jobParameters = parameters;
            _header = await GetScheduledTaskAsync(parameters.ScheduledTaskId);
            var _activityConnect = await GetDatabaseConnectionAsync(_header.IdDataProvider);

            _logTask = CreateTaskLog(parameters);
            await InsertLogTaskAsync(_logTask);

            if (!_header.TaskQueries.Any())
            {
                await HandleNoQueriesAsync(_logTask);
                return;
            }

            _taskId = _logTask.TaskLogId;
            await InsertLogTaskStepAsync("Initialization", $"Nbr of queries: {_header.TaskQueries.Count}", false);

            try
            {
                var _resultInfo = "Ok";
                await HandleDataTransferTaskAsync(_activityConnect);
                _resultInfo =
                    $"Rows bulkinserted: {_dataTransferStat.BulkInserted},Rows inserted: {_dataTransferStat.Inserted},Rows updated: {_dataTransferStat.Updated},Rows deleted: {_dataTransferStat.Deleted}";

                await FinalizeTaskAsync(_logTask, parameters.GenerateFiles, _resultInfo);
            }
            catch (Exception ex)
            {
                await HandleTaskErrorAsync(_logTask, ex);
            }

            await UpdateTaskLogAsync(_logTask);
            await _context.SaveChangesAsync("backgroundworker");
        }



        private async Task HandleDataTransferTaskAsync(DatabaseConnection _activityConnect)
        {
            foreach (var detail in _header.TaskQueries.OrderBy(a => a.ExecutionOrder))
            {
                await FetchData(detail, _activityConnect.DataTransferMaxNbrofRowsFetched);

                var _headerParameters = JsonSerializer.Deserialize<ScheduledTaskParameters>(_header.TaskParameters);
                int i = 0;
                foreach (var value in _fetchedData)
                {
                    await HandleDataTransferTask(detail, value.Value, _headerParameters.DataTransferId, i);
                    i++;
                }

                _fetchedData.Clear();
            }
        }

        private async ValueTask HandleDataTransferTask(ScheduledTaskQuery a, DataTable data, long activityIdTransfer,
    int loopNumber)
        {
            if (data.Rows.Count > 0)
            {
                var detailParam = JsonSerializer.Deserialize<ScheduledTaskQueryParameters>(a.ExecutionParameters!);
                var checkTableQuery = $@"IF (EXISTS (SELECT *
                                                       FROM INFORMATION_SCHEMA.TABLES
                                                       WHERE TABLE_SCHEMA = 'dbo'
                                                       AND TABLE_NAME = '{detailParam?.DataTransferTargetTableName}'))
                                                       BEGIN
                                                          select 1
                                                       END;
                                                    ELSE
                                                       BEGIN
                                                          select 0
                                                       END;";
                var result = await _dbReader.CkeckTableExists(checkTableQuery, activityIdTransfer);

                if (!result)
                {
                    string queryCreate;
                    if (detailParam!.DataTransferUsePk)
                    {
                        queryCreate = CreateSqlServerTableFromDatatable.CreateTableFromSchema(data,
                            detailParam.DataTransferTargetTableName, false, detailParam.DataTransferPk);
                    }
                    else
                    {
                        if (detailParam.DataTransferCommandBehaviour == DataTransferBasicBehaviour.Append.ToString())
                            queryCreate =
                                CreateSqlServerTableFromDatatable.CreateTableFromSchema(data,
                                    detailParam.DataTransferTargetTableName, false);
                        else
                            queryCreate =
                                CreateSqlServerTableFromDatatable.CreateTableFromSchema(data,
                                    detailParam.DataTransferTargetTableName, loopNumber == 0);
                    }

                    await _dbReader.CreateTable(queryCreate, activityIdTransfer);
                }

                if (!detailParam!.DataTransferUsePk)
                {
                    await _dbReader.LoadDatatableToTable(data, detailParam.DataTransferTargetTableName, activityIdTransfer);
                    _logTask.HasSteps = true;
                    _dataTransferStat.Inserted = +data.Rows.Count;
                    _dataTransferStat.BulkInserted = +data.Rows.Count;
                    await _context.AddAsync(new TaskStepLog
                    {
                        TaskLogId = _taskId,
                        Step = "Bulk insert completed",
                        Info = "Rows (command:" + detailParam.DataTransferCommandBehaviour + "): " +
                               data.Rows.Count
                    });
                }
                else
                {
                    var tempTable = "tmp_" + detailParam.DataTransferTargetTableName +
                                    DateTime.Now.ToString("yyyyMMddHHmmss");
                    var columnNames = new HashSet<string>(data.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                    var queryCreate =
                        CreateSqlServerTableFromDatatable.CreateTableFromSchema(data, tempTable, true,
                            detailParam.DataTransferPk);

                    await _dbReader.CreateTable(queryCreate, activityIdTransfer);
                    try
                    {
                        await _dbReader.LoadDatatableToTable(data, tempTable, activityIdTransfer);

                        _dataTransferStat.BulkInserted = +data.Rows.Count;
                        await _context.AddAsync(new TaskStepLog
                        {
                            TaskLogId = _taskId,
                            Step = "Bulk insert completed",
                            Info = "Rows (command:" + detailParam.DataTransferCommandBehaviour + "): " +
                                   data.Rows.Count
                        });
                    }
                    catch (Exception)
                    {
                        await _dbReader.DeleteTable(tempTable, activityIdTransfer);
                        throw;
                    }

                    var mergeSql = new
                    {
                        MERGE_FIELD_NAME = string.Join(" and ",
                            detailParam.DataTransferPk!.Select(name => $"target.[{name}] = source.[{name}]")),
                        FIELD_LIST = string.Join(", ", columnNames.Select(name => $"[{name}]")),
                        SOURCE_TABLE_NAME = tempTable,
                        TARGET_TABLE_NAME = detailParam.DataTransferTargetTableName,
                        UPDATES_LIST = string.Join(", ", columnNames.Select(name => $"target.[{name}] = source.[{name}]")),
                        SOURCE_FIELD_LIST = string.Join(", ", columnNames.Select(name => $"source.[{name}]")),
                        UPDATE_REQUIRED_EXPRESSION = string.Join(" OR ",
                            columnNames.Select(name =>
                                $"IIF((target.[{name}] IS NULL AND source.[{name}] IS NULL) OR target.[{name}] = source.[{name}], 1, 0) = 0")) // Take care around null values
                    };

                    string mergeSqlTemplate;

                    if (detailParam.DataTransferCommandBehaviour == DataTransferAdvancedBehaviour.Insert.ToString())
                        mergeSqlTemplate = @$"DECLARE @SummaryOfChanges TABLE(Change VARCHAR(20));
                                             MERGE INTO {mergeSql.TARGET_TABLE_NAME} WITH (HOLDLOCK) AS target
                                             USING (SELECT * FROM {mergeSql.SOURCE_TABLE_NAME}) as source
                                             ON ({mergeSql.MERGE_FIELD_NAME})
                                             WHEN NOT MATCHED THEN
                                                 INSERT ({mergeSql.FIELD_LIST}) VALUES ({mergeSql.SOURCE_FIELD_LIST})
                                             OUTPUT $action INTO @SummaryOfChanges;

                                             SELECT Change, COUNT(1) AS CountPerChange
                                             FROM @SummaryOfChanges
                                             GROUP BY Change;";
                    else if (detailParam.DataTransferCommandBehaviour ==
                             DataTransferAdvancedBehaviour.InsertOrUpdateOrDelete.ToString())
                        mergeSqlTemplate = @$"DECLARE @SummaryOfChanges TABLE(Change VARCHAR(20));
                                             MERGE INTO {mergeSql.TARGET_TABLE_NAME} WITH (HOLDLOCK) AS target
                                             USING (SELECT * FROM {mergeSql.SOURCE_TABLE_NAME}) as source
                                             ON ({mergeSql.MERGE_FIELD_NAME})
                                             WHEN MATCHED AND ({mergeSql.UPDATE_REQUIRED_EXPRESSION}) THEN
                                                 UPDATE SET {mergeSql.UPDATES_LIST}
                                             WHEN NOT MATCHED THEN
                                                 INSERT ({mergeSql.FIELD_LIST}) VALUES ({mergeSql.SOURCE_FIELD_LIST})
                                             WHEN NOT MATCHED BY SOURCE THEN
                                                 DELETE
                                             OUTPUT $action INTO @SummaryOfChanges;

                                             SELECT Change, COUNT(1) AS CountPerChange
                                             FROM @SummaryOfChanges
                                             GROUP BY Change;";
                    else
                        mergeSqlTemplate = @$"DECLARE @SummaryOfChanges TABLE(Change VARCHAR(20));
                                             MERGE INTO {mergeSql.TARGET_TABLE_NAME} WITH (HOLDLOCK) AS target
                                             USING (SELECT * FROM {mergeSql.SOURCE_TABLE_NAME}) as source
                                             ON ({mergeSql.MERGE_FIELD_NAME})
                                             WHEN MATCHED AND ({mergeSql.UPDATE_REQUIRED_EXPRESSION}) THEN
                                                 UPDATE SET {mergeSql.UPDATES_LIST}
                                             WHEN NOT MATCHED THEN
                                                 INSERT ({mergeSql.FIELD_LIST}) VALUES ({mergeSql.SOURCE_FIELD_LIST})
                                             OUTPUT $action INTO @SummaryOfChanges;

                                             SELECT Change, COUNT(1) AS CountPerChange
                                             FROM @SummaryOfChanges
                                             GROUP BY Change;";

                    var mergeResult = await _dbReader.MergeTables(mergeSqlTemplate, activityIdTransfer);

                    _dataTransferStat.Inserted = +mergeResult.InsertedCount;
                    _dataTransferStat.Updated = +mergeResult.UpdatedCount;
                    _dataTransferStat.Deleted = +mergeResult.DeletedCount;
                    await _dbReader.DeleteTable(tempTable, activityIdTransfer);
                    await _context.AddAsync(new TaskStepLog
                    {
                        TaskLogId = _taskId,
                        Step = "Merge completed",
                        Info = "Rows inserted: " + mergeResult.InsertedCount + " Rows updated: " +
                               mergeResult.UpdatedCount + " Rows deleted: " + mergeResult.DeletedCount
                    });
                }
            }
        }

    }
}
