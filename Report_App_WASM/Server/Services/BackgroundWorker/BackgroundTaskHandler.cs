using System.Net.Mail;
using System.Text.Json;
using Report_App_WASM.Server.Services.EmailSender;
using Report_App_WASM.Server.Services.FilesManagement;
using Report_App_WASM.Server.Services.RemoteDb;
using Report_App_WASM.Server.Utils.BackgroundWorker;
using Report_App_WASM.Server.Utils.FIles;

namespace Report_App_WASM.Server.Services.BackgroundWorker;

public class BackgroundTaskHandler : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IRemoteDatabaseActionsHandler _dbReader;
    private readonly IEmailSender _emailSender;
    private readonly LocalFilesService _fileDeposit;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly IMapper _mapper;
    private List<EmailRecipient> _emails = new();

    private Dictionary<ScheduledTaskQuery, DataTable> _fetchedData = new();
    private List<MemoryFileContainer> _fileResults = new();
    private ScheduledTask _header = null!;
    private TaskJobParameters _jobParameters = null!;

    private JsonSerializerOptions _jsonOpt = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private long _taskId;
    private TaskLog _logTask;

    private class DataTransferRowsStats
    {
        public int BulkInserted { get; set; }
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public int Deleted { get; set; }
    }

    DataTransferRowsStats _dataTransferStat = new DataTransferRowsStats();

    public BackgroundTaskHandler(ApplicationDbContext context, IEmailSender emailSender,
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

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public async ValueTask HandleTask(TaskJobParameters parameters)
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
            if (_header.Type != TaskType.DataTransfer)
            {
                await HandleNonDataTransferTaskAsync(_activityConnect);
            }
            else
            {
                await HandleDataTransferTaskAsync(_activityConnect);
                _resultInfo =
                    $"Rows bulkinserted: {_dataTransferStat.BulkInserted},Rows inserted: {_dataTransferStat.Inserted},Rows updated: {_dataTransferStat.Updated},Rows deleted: {_dataTransferStat.Deleted}";
            }

            await FinalizeTaskAsync(_logTask, parameters.GenerateFiles, _resultInfo);
        }
        catch (Exception ex)
        {
            await HandleTaskErrorAsync(_logTask, ex);
        }

        await UpdateTaskLogAsync(_logTask);
        await _context.SaveChangesAsync("backgroundworker");
    }

    private async Task<ScheduledTask> GetScheduledTaskAsync(long scheduledTaskId)
    {
        return await _context.ScheduledTask
            .Where(a => a.ScheduledTaskId == scheduledTaskId)
            .Include(a => a.DataProvider)
            .Include(a => a.TaskQueries)
            .Include(a => a.DistributionLists)
            .FirstOrDefaultAsync();
    }

    private async Task<DatabaseConnection> GetDatabaseConnectionAsync(long dataProviderId)
    {
        return await _context.DatabaseConnection
            .Where(a => a.DataProvider.DataProviderId == dataProviderId)
            .FirstOrDefaultAsync();
    }

    private TaskLog CreateTaskLog(TaskJobParameters parameters)
    {
        return new TaskLog
        {
            DataProviderId = _header.DataProvider.DataProviderId,
            ProviderName = _header.ProviderName,
            StartDateTime = DateTime.Now,
            JobDescription = _header.TaskName,
            Type = _header.Type.ToString(),
            Result = "Running",
            ScheduledTaskId = parameters.ScheduledTaskId,
            RunBy = _jobParameters.RunBy,
            EndDateTime = DateTime.Now
        };
    }

    private async Task InsertLogTaskAsync(TaskLog logTask)
    {
        await _context.AddAsync(logTask);
        await _context.SaveChangesAsync("backgroundworker");
    }

    private async Task HandleNoQueriesAsync(TaskLog logTask)
    {
        logTask.EndDateTime = DateTime.Now;
        logTask.Result = "No query to run";
        logTask.Error = true;
        logTask.DurationInSeconds = (int)(logTask.EndDateTime - logTask.StartDateTime).TotalSeconds;
        await _context.AddAsync(logTask);
        await _context.SaveChangesAsync("backgroundworker");
    }

    private async Task InsertLogTaskStepAsync(string step, string info, bool error)
    {
        _logTask.HasSteps = true;
        await _context.AddAsync(new TaskStepLog { TaskLogId = _taskId, Step = step, Info = info, Error = error });
        await _context.SaveChangesAsync("backgroundworker");
    }

    private async Task HandleNonDataTransferTaskAsync(DatabaseConnection _activityConnect)
    {
        if (_header.SendByEmail && _header.DistributionLists.Select(a => a.Recipients).FirstOrDefault() != "[]")
            _emails = JsonSerializer.Deserialize<List<EmailRecipient>>(_header.DistributionLists
                .Select(a => a.Recipients).FirstOrDefault()!);
        if (_jobParameters.ManualRun) _emails = _jobParameters.CustomEmails;

        foreach (var detail in _header.TaskQueries.OrderBy(a => a.ExecutionOrder))
        {
            await FetchData(detail, _activityConnect.TaskSchedulerMaxNbrofRowsFetched);
            await _context.SaveChangesAsync("backgroundworker");
        }

        if (_header.Type == TaskType.Alert)
        {
            await HandleTaskAlertAsync();
        }
        else
        {
            await GenerateFile();
            foreach (var f in _fileResults)
                await WriteFileAsync(f, f.FileName, _jobParameters.GenerateFiles, f.FileName);
            await GenerateEmail();
            _fileResults.Clear();
        }

        _fetchedData.Clear();
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

    private async Task FinalizeTaskAsync(TaskLog logTask, bool generateFiles, string _result = "Ok")
    {
        logTask.Error = false;
        logTask.Result = _result;
        logTask.EndDateTime = DateTime.Now;
        logTask.DurationInSeconds = (int)(logTask.EndDateTime - logTask.StartDateTime).TotalSeconds;
        if (generateFiles)
        {
            _header.LastRunDateTime = DateTime.Now;
            _context.Entry(_header).State = EntityState.Modified;
        }

        await InsertLogTaskStepAsync("Job end", $"Total duration {logTask.DurationInSeconds} seconds", false);
    }

    private async Task HandleTaskErrorAsync(TaskLog logTask, Exception ex)
    {
        logTask.Result = new string(ex.Message.Take(449).ToArray());
        logTask.Error = true;
        logTask.EndDateTime = DateTime.Now;
        logTask.DurationInSeconds = (int)(logTask.EndDateTime - logTask.StartDateTime).TotalSeconds;
        logTask.Result = ex.Message.Length > 440 ? ex.Message.Substring(0, 440) : ex.Message;
        await _emailSender.GenerateErrorEmailAsync(ex.Message, _header.ProviderName + ": " + _header.TaskName);
        await InsertLogTaskStepAsync("Error", logTask.Result, true);
        _fetchedData.Clear();
    }

    private async Task UpdateTaskLogAsync(TaskLog logTask)
    {
        _context.Update(logTask);
        await _context.SaveChangesAsync("backgroundworker");
    }

    private async ValueTask FetchData(ScheduledTaskQuery detail, int maxRows = 100000)
    {
        using var remoteDb = new RemoteDatabaseActionsHandler(_context, _mapper);
        var detailParam =
            JsonSerializer.Deserialize<ScheduledTaskQueryParameters>(detail.ExecutionParameters!, _jsonOpt);
        List<QueryCommandParameter>? param = new();
        if (_jobParameters.CustomQueryParameters!.Any())
            param = _jobParameters.CustomQueryParameters;
        else if (_header.UseGlobalQueryParameters && _header.GlobalQueryParameters != "[]" &&
                 !string.IsNullOrEmpty(_header.GlobalQueryParameters))
            param = JsonSerializer.Deserialize<List<QueryCommandParameter>>(_header.GlobalQueryParameters, _jsonOpt);

        if (detail.QueryParameters != "[]" && !string.IsNullOrEmpty(detail.QueryParameters))
        {
            var desParam = JsonSerializer.Deserialize<List<QueryCommandParameter>>(detail.QueryParameters, _jsonOpt);
            foreach (var value in desParam!)
                if (param!.All(a => a.ParameterIdentifier?.ToLower() != value.ParameterIdentifier?.ToLower()))
                    param?.Add(value);
        }

        var table = await remoteDb.RemoteDbToDatableAsync(
            new RemoteDbCommandParameters
            {
                DataProviderId = _header.DataProvider.DataProviderId,
                ScheduledTaskId = _header.ScheduledTaskId,
                ScheduledTaskQueryId = detail.ScheduledTaskQueryId,
                QueryToRun = detail.Query,
                QueryInfo = detail.QueryName,
                PaginatedResult = true,
                LastRunDateTime = detail.LastRunDateTime ?? DateTime.Now,
                QueryCommandParameters = param,
                MaxSize = maxRows
            }, _jobParameters.Cts, _taskId);
        _logTask.HasSteps = true;

        if (detailParam!.GenerateIfEmpty || table.Rows.Count > 0) _fetchedData.Add(detail, table);

        if (_jobParameters.GenerateFiles)
        {
            detail.LastRunDateTime = DateTime.Now;
            _context.Entry(detail).State = EntityState.Modified;
        }
    }

    private async ValueTask WriteFileAsync(MemoryFileContainer fileResult, string fName, bool useDepositConfiguration,
        string? subName = null)
    {
        var localFileResult = await _fileDeposit.SaveFileForBackupAsync(fileResult, fName);
        if (!localFileResult.Success)
            await _emailSender.GenerateErrorEmailAsync(localFileResult.Message, "Local file writing: ");

        ReportGenerationLog filecreationLocal = new()
        {
            DataProviderId = _header.DataProvider.DataProviderId,
            ProviderName = _header.ProviderName,
            CreatedAt = DateTime.Now,
            CreatedBy = "Report Service",
            ScheduledTaskId = _header.ScheduledTaskId,
            TaskLogId = _taskId,
            ReportName = _header.TaskName,
            FileType = _header.TypeFile.ToString(),
            ReportPath = "/docsstorage/" + fName,
            FileName = fName,
            IsAvailable = true,
            Error = !localFileResult.Success,
            Result = localFileResult.Message, FileGenerationType = FileGenerationType.LocalCopy,
            FileSizeInMb = BytesConverter.ConvertBytesToMegabytes(fileResult.Content.Length)
        };
        await _context.AddAsync(filecreationLocal);
        await _context.SaveChangesAsync("backgroundworker");
        _logTask.HasSteps = true;
        await _context.AddAsync(new TaskStepLog
        {
            TaskLogId = _taskId, Step = "File local storage", Info = "Ok", RelatedLogType = LogType.ReportGenerationLog,
            RelatedLogId = filecreationLocal.Id
        });

        if (_header.FileStorageLocationId != 0 && useDepositConfiguration && localFileResult.Success)
        {
            string completePath;
            SubmitResult resultDeposit;
            var config = await _context.FileStorageLocation.Include(a => a.SftpConfiguration).AsNoTracking()
                .FirstAsync(a => a.FileStorageLocationId == _header.FileStorageLocationId);
            ReportGenerationLog filecreationRemote = new()
            {
                DataProviderId = _header.DataProvider.DataProviderId,
                ProviderName = _header.ProviderName,
                CreatedAt = DateTime.Now,
                CreatedBy = "Report Service",
                ScheduledTaskId = _header.ScheduledTaskId,
                TaskLogId = _taskId,
                ReportName = _header.TaskName,
                FileType = _header.TypeFile.ToString(),
                FileName = fName,
                IsAvailable = false,
                FileSizeInMb = filecreationLocal.FileSizeInMb
            };
            await _context.AddAsync(filecreationRemote);
            await _context.SaveChangesAsync("backgroundworker");
            if (config is { SftpConfiguration: not null, UseSftpProtocol: true })
            {
                var storagePath = Path.Combine(_hostingEnvironment.WebRootPath, "docsstorage");
                var localfilePath = Path.Combine(storagePath, fName);
                if (config.SftpConfiguration.UseFtpProtocol)
                {
                    filecreationRemote.FileGenerationType = FileGenerationType.Ftp;
                    completePath = "FTP Host:" + config.SftpConfiguration.Host + " Path:" + config.FilePath;
                    using var ftp = new FtpService(_context);
                    resultDeposit = await ftp.UploadFileAsync(config.SftpConfiguration.SftpConfigurationId,
                        localfilePath, config.FilePath, fName, config.TryToCreateFolder);
                    await _context.AddAsync(new TaskStepLog
                    {
                        TaskLogId = _taskId, Step = "File FTP drop", Info = config.FilePath,
                        RelatedLogType = LogType.ReportGenerationLog, RelatedLogId = filecreationRemote.Id
                    });
                }
                else
                {
                    filecreationRemote.FileGenerationType = FileGenerationType.Sftp;
                    completePath = "Sftp Host:" + config.SftpConfiguration.Host + " Path:" + config.FilePath;
                    using var sftp = new SftpService(_context);
                    resultDeposit = await sftp.UploadFileAsync(config.SftpConfiguration.SftpConfigurationId,
                        localfilePath, config.FilePath, fName, config.TryToCreateFolder);
                    await _context.AddAsync(new TaskStepLog
                    {
                        TaskLogId = _taskId, Step = "File Sftp drop", Info = config.FilePath,
                        RelatedLogType = LogType.ReportGenerationLog, RelatedLogId = filecreationRemote.Id
                    });
                }
            }
            else
            {
                filecreationRemote.FileGenerationType = FileGenerationType.DirectToFolder;
                completePath = Path.Combine(config.FilePath, fName);
                resultDeposit =
                    await _fileDeposit.SaveFileAsync(fileResult, fName, config.FilePath, config.TryToCreateFolder);
                await _context.AddAsync(new TaskStepLog
                {
                    TaskLogId = _taskId, Step = "File folder drop", Info = config.FilePath,
                    RelatedLogType = LogType.ReportGenerationLog, RelatedLogId = filecreationRemote.Id
                });
            }

            if (!resultDeposit.Success)
            {
                await _emailSender.GenerateErrorEmailAsync(resultDeposit.Message, "File deposit: ");
                await _context.AddAsync(new TaskStepLog
                {
                    TaskLogId = _taskId, Step = "Error", Info = resultDeposit.Message,
                    RelatedLogType = LogType.ReportGenerationLog, RelatedLogId = filecreationRemote.Id, Error = true
                });
            }

            filecreationRemote.ReportPath = completePath;
            filecreationRemote.Error = !resultDeposit.Success;
            filecreationRemote.Result = resultDeposit.Message;

            _context.Update(filecreationRemote);
            await _context.SaveChangesAsync("backgroundworker");
        }
    }

    private async ValueTask GenerateFile()
    {
        string fName;
        string fExtension;
        var informationSheet = false;
        List<ExcelCreationDatatable> excelMultipleTabs = new();
        var headerParam = JsonSerializer.Deserialize<ScheduledTaskParameters>(_header.TaskParameters, _jsonOpt);

        foreach (var d in _fetchedData)
        {
            MemoryFileContainer fileCreated;
            var detailParam =
                JsonSerializer.Deserialize<ScheduledTaskQueryParameters>(d.Key.ExecutionParameters!, _jsonOpt);
            if (string.IsNullOrEmpty(detailParam?.FileName))
                fName =
                    $"{_header.ProviderName.RemoveSpecialExceptSpaceCharacters()}-{d.Key.QueryName.RemoveSpecialExceptSpaceCharacters()}_{DateTime.Now:yyyyMMdd_HHmmss}";
            else
                fName = $"{detailParam.FileName.RemoveSpecialExceptSpaceCharacters()}_{DateTime.Now:yyyyMMdd_HHmmss}";

            if (_header.TypeFile == FileType.Excel)
            {
                if (detailParam!.SeparateExcelFile)
                {
                    fExtension = ".xlsx";
                    fName += fExtension;
                    List<ExcelCreationDatatable> excelCreationDatatables = new()
                    {
                        new ExcelCreationDatatable
                            { TabName = detailParam.ExcelTabName ?? d.Key.QueryName, Data = d.Value }
                    };
                    ExcelCreationData dataExcel = new()
                    {
                        FileName = fName,
                        Data = excelCreationDatatables,
                        ValidationSheet = detailParam.AddValidationSheet,
                        ValidationText = headerParam?.ValidationSheetText
                    };

                    fileCreated = CreateFile.ExcelFromSeveralsDatable(dataExcel);
                    dataExcel.Dispose();
                }
                else
                {
                    string? tabName;
                    ExcelTemplate template = new();
                    if (!informationSheet) informationSheet = detailParam.AddValidationSheet;
                    if (headerParam!.UseAnExcelTemplate)
                    {
                        template = detailParam.ExcelTemplate;
                        if (string.IsNullOrEmpty(template.ExcelTabName))
                            throw new NullReferenceException(
                                $"Tabname of excel template for query {d.Key.QueryName} is null.");
                        tabName = template.ExcelTabName;
                    }
                    else
                    {
                        tabName = detailParam.ExcelTabName ?? d.Key.QueryName;
                    }

                    excelMultipleTabs.Add(new ExcelCreationDatatable
                        { TabName = tabName, ExcelTemplate = template, Data = d.Value });
                    continue;
                }
            }
            else if (_header.TypeFile == FileType.Json)
            {
                fExtension = ".json";
                fName += fExtension;
                fileCreated = CreateFile.JsonFromDatable(fName, d.Value, detailParam!.EncodingType ?? "UTF8");
            }
            else if (_header.TypeFile == FileType.Csv)
            {
                fExtension = ".csv";
                fName += fExtension;
                fileCreated = CreateFile.CsvFromDatable(fName, d.Value, detailParam!.EncodingType,
                    headerParam?.Delimiter, detailParam.RemoveHeader);
            }
            else
            {
                fExtension = ".xml";
                fName += fExtension;
                fileCreated = CreateFile.XmlFromDatable(d.Key.QueryName, fName, detailParam?.EncodingType, d.Value);
            }

            _fileResults.Add(fileCreated);
            await _context.AddAsync(new TaskStepLog
                { TaskLogId = _taskId, Step = "File created", Info = fName });
        }

        if (excelMultipleTabs.Any())
        {
            fName = string.IsNullOrEmpty(headerParam?.ExcelFileName)
                ? $"{_header.ProviderName.RemoveSpecialExceptSpaceCharacters()}-{_header.TaskName.RemoveSpecialExceptSpaceCharacters()}_{DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx"}"
                : $"{headerParam.ExcelFileName.RemoveSpecialExceptSpaceCharacters()}_{DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx"}";

            MemoryFileContainer fileCreated;
            if (!headerParam!.UseAnExcelTemplate)
            {
                ExcelCreationData dataExcel = new()
                {
                    FileName = fName,
                    Data = excelMultipleTabs,
                    ValidationSheet = informationSheet,
                    ValidationText = headerParam.ValidationSheetText
                };

                fileCreated = CreateFile.ExcelFromSeveralsDatable(dataExcel);
                dataExcel.Dispose();
            }
            else
            {
                ExcelCreationData dataExcel = new()
                {
                    FileName = fName,
                    Data = excelMultipleTabs,
                    ValidationSheet = informationSheet,
                    ValidationText = headerParam.ValidationSheetText
                };
                var fileInfo = _fileDeposit.GetFileInfo(headerParam.ExcelTemplatePath);
                fileCreated = CreateFile.ExcelTemplateFromSeveralDatable(dataExcel, fileInfo);
                dataExcel.Dispose();
            }

            _fileResults.Add(fileCreated);
            excelMultipleTabs.Clear();
            await _context.AddAsync(new TaskStepLog
                { TaskLogId = _taskId, Step = "File created", Info = fName });
        }

        _fetchedData.Clear();
    }

    private async ValueTask HandleTaskAlertAsync()
    {
        var headerParam = JsonSerializer.Deserialize<ScheduledTaskParameters>(_header.TaskParameters, _jsonOpt);
        if (_fetchedData.Any())
        {
            var sendEmail = false;
            var a = _fetchedData.Select(a => a.Key).FirstOrDefault();
            if ((!headerParam!.AlertOccurenceByTime &&
                 a!.ExecutionCount > headerParam.NbrOfOccurencesBeforeResendAlertEmail - 1) ||
                a!.ExecutionCount == 0)
            {
                a.ExecutionCount = 0;
                sendEmail = true;
            }

            if (headerParam.AlertOccurenceByTime & (a.LastRunDateTime <
                                                    DateTime.Now.AddMinutes(-headerParam
                                                        .NbrOfMinutesBeforeResendAlertEmail))) sendEmail = true;
            if (sendEmail || _jobParameters.ManualRun)
            {
                var emailPrefix = await _context.SystemParameters.Select(a => a.AlertEmailPrefix)
                    .FirstOrDefaultAsync();
                if (_header.DistributionLists.Select(a => a.EmailMessage).FirstOrDefault() != "[]")
                {
                    var subject = emailPrefix + " - " + a.ScheduledTask?.ProviderName + ": " +
                                  a.ScheduledTask?.TaskName;
                    var message = "";
                    List<Attachment> listAttach = new();
                    foreach (var table in _fetchedData.Where(keyValuePair => keyValuePair.Value.Rows.Count > 0))
                        if (table.Value.Rows.Count < 101)
                        {
                            var valueMessage = table.Key.QueryName + ":" + Environment.NewLine;
                            valueMessage += table.Value.ToHtml();
                            message += "\r\n{0}";
                            message = string.Format(message, valueMessage);
                        }
                        else
                        {
                            ExcelCreationDatatable dataExcel = new()
                                { TabName = a.ScheduledTask?.TaskName, Data = table.Value };
                            var fileResult =
                                CreateFile.ExcelFromDatable((string?)(a.ScheduledTask?.TaskName), dataExcel);
                            var fName =
                                $"{_header.ProviderName.RemoveSpecialExceptSpaceCharacters()}-{table.Key.QueryName.RemoveSpecialExceptSpaceCharacters()}_{DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx"}";
                            listAttach.Add(new Attachment(new MemoryStream(fileResult.Content), fName,
                                fileResult.ContentType));
                        }

                    message = string.Format(_header.DistributionLists.Select(a => a.EmailMessage).FirstOrDefault()!,
                        message);

                    var result = await _emailSender.SendEmailAsync(_emails, subject, message, listAttach);
                    if (result.Success)
                        await _context.AddAsync(new TaskStepLog
                        {
                            TaskLogId = _taskId, Step = "Email sent", Info = subject, RelatedLogType = LogType.EmailLog,
                            RelatedLogId = result.KeyValue
                        });
                    else
                        await _context.AddAsync(new TaskStepLog
                            { TaskLogId = _taskId, Step = "Email not sent", Info = result.Message, Error = true });

                    listAttach.Clear();
                }

                _header.TaskQueries.ToList().ForEach(a => a.LastRunDateTime = DateTime.Now);
            }

            a.ExecutionCount++;
            //header.TaskDetails.ToList().ForEach(a => a.NbrOFCumulativeOccurences = nbrOccurrences);
            _context.Entry(a).State = EntityState.Modified;
        }
        else
        {
            var a = _header.TaskQueries.FirstOrDefault();
            a!.ExecutionCount = 0;
            _context.Entry(a).State = EntityState.Modified;
        }
    }

    private async Task GenerateEmail()
    {
        if (_emails.Any() && _fileResults.Any())
        {
            var emailPrefix = await _context.SystemParameters.Select(a => a.EmailPrefix).FirstOrDefaultAsync();
            var subject = emailPrefix + " - " + _header.ProviderName + ": " + _header.TaskName;

            List<Attachment> listAttach = new();
            listAttach.AddRange(_fileResults.Select(a =>
                new Attachment(new MemoryStream(a.Content), a.FileName, a.ContentType)).ToList());

            var message = _header.DistributionLists.Select(a => a.EmailMessage).FirstOrDefault();
            if (listAttach.Any())
                if (message != null)
                {
                    var result = await _emailSender.SendEmailAsync(_emails, subject, message, listAttach);
                    if (result.Success)
                        await _context.AddAsync(new TaskStepLog
                        {
                            TaskLogId = _taskId, Step = "Email sent", Info = subject, RelatedLogType = LogType.EmailLog,
                            RelatedLogId = result.KeyValue
                        });
                    else
                        await _context.AddAsync(new TaskStepLog
                            { TaskLogId = _taskId, Step = "Email not sent", Info = result.Message, Error = true });
                }
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