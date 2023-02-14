using System.Data;
using System.Net.Mail;
using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Services.EmailSender;
using Report_App_WASM.Server.Services.FilesManagement;
using Report_App_WASM.Server.Services.RemoteDb;
using Report_App_WASM.Server.Utils;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.Extensions;
using Report_App_WASM.Shared.RemoteQueryParameters;
using Report_App_WASM.Shared.SerializedParameters;

namespace Report_App_WASM.Server.Services.BackgroundWorker;

public class BackgroundTaskHandler : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IRemoteDbConnection _dbReader;
    private readonly IEmailSender _emailSender;
    private readonly LocalFilesService _fileDeposit;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly IMapper _mapper;
    private List<EmailRecipient>? _emails = new();

    private Dictionary<TaskDetail, DataTable> _fetchedData = new();
    private List<FileContentResult> _fileResults = new();
    private TaskHeader _header = null!;
    private TaskJobParameters _jobParameters = null!;

    private JsonSerializerOptions _jsonOpt = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private int _taskId;

    public BackgroundTaskHandler(
        ApplicationDbContext context, IEmailSender emailSender, IRemoteDbConnection dbReader,
        LocalFilesService fileDeposit, IMapper mapper, IWebHostEnvironment hostingEnvironment)
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
        GC.ReRegisterForFinalize(this);
    }

    public async Task HandleTask(TaskJobParameters parameters)
    {
        _jobParameters = parameters;
        _header = (await _context.TaskHeader.Where(a => a.TaskHeaderId == parameters.TaskHeaderId)
            .Include(a => a.Activity).Include(a => a.TaskDetails).Include(a => a.TaskEmailRecipients)
            .FirstOrDefaultAsync())!;
        var _activityConnect = await _context.ActivityDbConnection
            .Where(a => a.Activity.ActivityId == _header.IdActivity).FirstOrDefaultAsync();
        ApplicationLogTask logTask = new()
        {
            ActivityId = _header.Activity.ActivityId, ActivityName = _header.ActivityName, StartDateTime = DateTime.Now,
            JobDescription = _header.TaskName, Type = _header.Type + " service", Result = "Running",
            RunBy = _jobParameters.RunBy, EndDateTime = DateTime.Now
        };

        if (!_header.TaskDetails.Any())
        {
            logTask.EndDateTime = DateTime.Now;
            logTask.Result = "No query to run";
            logTask.Error = true;
            logTask.DurationInSeconds = (int)(logTask.EndDateTime - logTask.StartDateTime).TotalSeconds;
            await _context.AddAsync(logTask);
            await _context.SaveChangesAsync("backgroundworker");
            return;
        }

        await _context.AddAsync(logTask);
        await _context.SaveChangesAsync("backgroundworker");
        _taskId = logTask.Id;
        await _context.AddAsync(new ApplicationLogTaskDetails
            { TaskId = _taskId, Step = "Initialization", Info = "Nbr of queries:" + _header.TaskDetails.Count });

        try
        {
            if (_header.Type != TaskType.DataTransfer)
            {
                if (_header.SendByEmail && _header.TaskEmailRecipients.Select(a => a.Email).FirstOrDefault() != "[]")
                    _emails = JsonSerializer.Deserialize<List<EmailRecipient>>(_header.TaskEmailRecipients
                        .Select(a => a.Email).FirstOrDefault()!);
                if (_jobParameters.ManualRun) _emails = _jobParameters.CustomEmails;
                foreach (var detail in _header.TaskDetails.OrderBy(a => a.DetailSequence))
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
                        await WriteFileAsync(f, f.FileDownloadName, _jobParameters.GenerateFiles, f.FileDownloadName);
                    await GenerateEmail();
                    _fileResults.Clear();
                }

                _fetchedData.Clear();
            }
            else
            {
                foreach (var detail in _header.TaskDetails.OrderBy(a => a.DetailSequence))
                {
                    ApplicationLogTask log = new()
                    {
                        ActivityId = _header.Activity.ActivityId, ActivityName = _header.ActivityName,
                        StartDateTime = DateTime.Now, JobDescription = detail.QueryName,
                        Type = _header.Type + " service", Error = false, RunBy = _jobParameters.RunBy
                    };

                    await FetchData(detail, _activityConnect.DataTransferMaxNbrofRowsFetched);
                    await _context.AddAsync(log);
                    await _context.SaveChangesAsync("backgroundworker");
                    log.TaskId = log.Id;
                    var _headerParameters =
                        JsonSerializer.Deserialize<TaskHeaderParameters>(_header.TaskHeaderParameters);

                    foreach (var value in _fetchedData)
                        await HandleDataTransferTask(detail, value.Value, log, _headerParameters.DataTransferId);

                    log.EndDateTime = DateTime.Now;
                    log.DurationInSeconds = (int)(log.EndDateTime - log.StartDateTime).TotalSeconds;
                    _context.Update(log);
                    await _context.SaveChangesAsync("backgroundworker");
                    _fetchedData.Clear();
                }
            }

            logTask.Error = false;
            logTask.Result = "Ok";
            logTask.TaskId = logTask.Id;
            logTask.EndDateTime = DateTime.Now;
            logTask.DurationInSeconds = (int)(logTask.EndDateTime - logTask.StartDateTime).TotalSeconds;
            if (parameters.GenerateFiles)
            {
                _header.LastRunDateTime = DateTime.Now;
                _context.Entry(_header).State = EntityState.Modified;
            }

            await _context.AddAsync(new ApplicationLogTaskDetails
                { TaskId = _taskId, Step = "Job end", Info = $"Total duration {logTask.DurationInSeconds} seconds" });
        }
        catch (Exception ex)
        {
            logTask.Result = new string(ex.Message.Take(449).ToArray());
            logTask.Error = true;
            logTask.EndDateTime = DateTime.Now;
            logTask.DurationInSeconds = (int)(logTask.EndDateTime - logTask.StartDateTime).TotalSeconds;
            await _emailSender.GenerateErrorEmailAsync(ex.Message, _header.ActivityName + ": " + _header.TaskName);
            await _context.AddAsync(new ApplicationLogTaskDetails
                { TaskId = _taskId, Step = "Error", Info = logTask.Result });
            _fetchedData.Clear();
            GC.SuppressFinalize(this);
        }

        _context.Update(logTask);
        await _context.SaveChangesAsync("backgroundworker");
    }


    private async Task FetchData(TaskDetail detail, int maxRows = 100000)
    {
        using var remoteDb = new RemoteDbConnection(_context, _mapper);
        var detailParam = JsonSerializer.Deserialize<TaskDetailParameters>(detail.TaskDetailParameters!, _jsonOpt);
        List<QueryCommandParameter>? param = new();
        if (_jobParameters.CustomQueryParameters!.Any())
            param = _jobParameters.CustomQueryParameters;
        else if (_header.UseGlobalQueryParameters && _header.QueryParameters != "[]" &&
                 !string.IsNullOrEmpty(_header.QueryParameters))
            param = JsonSerializer.Deserialize<List<QueryCommandParameter>>(_header.QueryParameters, _jsonOpt);

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
                ActivityId = _header.Activity.ActivityId, QueryToRun = detail.Query, QueryInfo = detail.QueryName,
                PaginatedResult = true, LastRunDateTime = detail.LastRunDateTime ?? DateTime.Now,
                QueryCommandParameters = param, MaxSize = maxRows
            }, _jobParameters.Cts, _taskId);
        await _context.AddAsync(new ApplicationLogTaskDetails
        {
            TaskId = _taskId, Step = "Fetch data completed",
            Info = detail.QueryName + "- Nbr of rows:" + table.Rows.Count
        });

        if (detailParam!.GenerateIfEmpty || table.Rows.Count > 0) _fetchedData.Add(detail, table);

        if (_jobParameters.GenerateFiles)
        {
            detail.LastRunDateTime = DateTime.Now;
            _context.Entry(detail).State = EntityState.Modified;
        }
    }

    private async Task WriteFileAsync(FileContentResult fileResult, string fName, bool useDepositConfiguration,
        string? subName = null)
    {
        var localFileResult = await _fileDeposit.SaveFileForBackupAsync(fileResult, fName);
        if (!localFileResult.Success)
            await _emailSender.GenerateErrorEmailAsync(localFileResult.Message, "Local file writing: ");

        ApplicationLogReportResult filecreationLocal = new()
        {
            ActivityId = _header.Activity.ActivityId,
            ActivityName = _header.ActivityName,
            CreatedAt = DateTime.Now,
            CreatedBy = "Report Service",
            TaskHeaderId = _header.TaskHeaderId,
            ReportName = _header.TaskName,
            SubName = subName ?? "",
            FileType = _header.TypeFile.ToString(),
            ReportPath = "/docsstorage/" + fName,
            FileName = fName,
            IsAvailable = true,
            Error = !localFileResult.Success,
            Result = localFileResult.Message,
            FileSizeInMb = BytesConverter.ConvertBytesToMegabytes(fileResult.FileContents.Length)
        };
        await _context.AddAsync(filecreationLocal);
        await _context.AddAsync(new ApplicationLogTaskDetails
            { TaskId = _taskId, Step = "File local storage", Info = "Ok" });

        if (_header.FileDepositPathConfigurationId != 0 && useDepositConfiguration && localFileResult.Success)
        {
            string completePath;
            SubmitResult resultDeposit;
            var config = await _context.FileDepositPathConfiguration.Include(a => a.SftpConfiguration).AsNoTracking()
                .FirstAsync(a => a.FileDepositPathConfigurationId == _header.FileDepositPathConfigurationId);
            if (config.SftpConfiguration != null && config.UseSftpProtocol)
            {
                var storagePath = Path.Combine(_hostingEnvironment.WebRootPath, "docsstorage");
                var localfilePath = Path.Combine(storagePath, fName);
                if (config.SftpConfiguration.UseFtpProtocol)
                {
                    completePath = "FTP Host:" + config.SftpConfiguration.Host + " Path:" + config.FilePath;
                    using var ftp = new FtpService(_context);
                    resultDeposit = await ftp.UploadFileAsync(config.SftpConfiguration.SftpConfigurationId,
                        localfilePath, config.FilePath, fName, config.TryToCreateFolder);
                    await _context.AddAsync(new ApplicationLogTaskDetails
                        { TaskId = _taskId, Step = "File FTP drop", Info = config.FilePath });
                }
                else
                {
                    completePath = "Sftp Host:" + config.SftpConfiguration.Host + " Path:" + config.FilePath;
                    using var sftp = new SftpService(_context);
                    resultDeposit = await sftp.UploadFileAsync(config.SftpConfiguration.SftpConfigurationId,
                        localfilePath, config.FilePath, fName, config.TryToCreateFolder);
                    await _context.AddAsync(new ApplicationLogTaskDetails
                        { TaskId = _taskId, Step = "File Sftp drop", Info = config.FilePath });
                }
            }
            else
            {
                completePath = Path.Combine(config.FilePath, fName);
                resultDeposit =
                    await _fileDeposit.SaveFileAsync(fileResult, fName, config.FilePath, config.TryToCreateFolder);
                await _context.AddAsync(new ApplicationLogTaskDetails
                    { TaskId = _taskId, Step = "File folder drop", Info = config.FilePath });
            }

            if (!resultDeposit.Success)
                await _emailSender.GenerateErrorEmailAsync(resultDeposit.Message, "File deposit: ");

            ApplicationLogReportResult filecreationRemote = new()
            {
                ActivityId = _header.Activity.ActivityId,
                ActivityName = _header.ActivityName,
                CreatedAt = DateTime.Now,
                CreatedBy = "Report Service",
                TaskHeaderId = _header.TaskHeaderId,
                ReportName = _header.TaskName,
                SubName = subName ?? "",
                FileType = _header.TypeFile.ToString(),
                ReportPath = completePath,
                FileName = fName,
                IsAvailable = false,
                Error = !resultDeposit.Success,
                Result = resultDeposit.Message,
                FileSizeInMb = filecreationLocal.FileSizeInMb
            };
            await _context.AddAsync(filecreationRemote);
        }
    }

    private async Task HandleDataTransferTask(TaskDetail a, DataTable data, ApplicationLogTask logTask,
        int activityIdTransfer)
    {
        if (data.Rows.Count > 0)
        {
            var detailParam = JsonSerializer.Deserialize<TaskDetailParameters>(a.TaskDetailParameters!);
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
                                detailParam.DataTransferTargetTableName, true);
                }

                await _dbReader.CreateTable(queryCreate, activityIdTransfer);
            }

            if (!detailParam!.DataTransferUsePk)
            {
                logTask.Result = "Lines inserted (command:" + detailParam.DataTransferCommandBehaviour + "): " +
                                 data.Rows.Count;
                await _dbReader.LoadDatatableToTable(data, detailParam.DataTransferTargetTableName, activityIdTransfer);
                await _context.AddAsync(new ApplicationLogTaskDetails
                {
                    TaskId = _taskId, Step = "Bulk insert completed",
                    Info = "Lines inserted (command:" + detailParam.DataTransferCommandBehaviour + "): " +
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
                    await _context.AddAsync(new ApplicationLogTaskDetails
                    {
                        TaskId = _taskId, Step = "Bulk insert completed",
                        Info = "Lines inserted (command:" + detailParam.DataTransferCommandBehaviour + "): " +
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

                logTask.Result = "Nbr lines inserted: " + mergeResult.InsertedCount + " Nbr lines updated: " +
                                 mergeResult.UpdatedCount + " Nbr lines deleted: " + mergeResult.DeletedCount;
                await _dbReader.DeleteTable(tempTable, activityIdTransfer);
                await _context.AddAsync(new ApplicationLogTaskDetails
                {
                    TaskId = _taskId, Step = "Merge completed",
                    Info = "Nbr lines inserted: " + mergeResult.InsertedCount + " Nbr lines updated: " +
                           mergeResult.UpdatedCount + " Nbr lines deleted: " + mergeResult.DeletedCount
                });
            }
        }
    }

    private async Task GenerateFile()
    {
        string fName;
        string fExtension;
        var informationSheet = false;
        List<ExcelCreationDatatable> excelMultipleTabs = new();
        var headerParam = JsonSerializer.Deserialize<TaskHeaderParameters>(_header.TaskHeaderParameters, _jsonOpt);

        foreach (var d in _fetchedData)
        {
            FileContentResult fileCreated;
            var detailParam = JsonSerializer.Deserialize<TaskDetailParameters>(d.Key.TaskDetailParameters!, _jsonOpt);
            if (string.IsNullOrEmpty(detailParam?.FileName))
                fName =
                    $"{_header.ActivityName.RemoveSpecialExceptSpaceCharacters()}-{d.Key.QueryName.RemoveSpecialExceptSpaceCharacters()}_{DateTime.Now:yyyyMMdd_HHmmss}";
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
                        FileName = fName, Data = excelCreationDatatables,
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
            await _context.AddAsync(new ApplicationLogTaskDetails
                { TaskId = _taskId, Step = "File created", Info = fName });
        }

        if (excelMultipleTabs.Any())
        {
            fName = string.IsNullOrEmpty(headerParam?.ExcelFileName)
                ? $"{_header.ActivityName.RemoveSpecialExceptSpaceCharacters()}-{_header.TaskName.RemoveSpecialExceptSpaceCharacters()}_{DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx"}"
                : $"{headerParam.ExcelFileName.RemoveSpecialExceptSpaceCharacters()}_{DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx"}";

            FileContentResult fileCreated;
            if (!headerParam!.UseAnExcelTemplate)
            {
                ExcelCreationData dataExcel = new()
                {
                    FileName = fName, Data = excelMultipleTabs, ValidationSheet = informationSheet,
                    ValidationText = headerParam.ValidationSheetText
                };

                fileCreated = CreateFile.ExcelFromSeveralsDatable(dataExcel);
                dataExcel.Dispose();
            }
            else
            {
                ExcelCreationData dataExcel = new()
                {
                    FileName = fName, Data = excelMultipleTabs, ValidationSheet = informationSheet,
                    ValidationText = headerParam.ValidationSheetText
                };
                var fileInfo = _fileDeposit.GetFileInfo(headerParam.ExcelTemplatePath);
                fileCreated = CreateFile.ExcelTemplateFromSeveralDatable(dataExcel, fileInfo);
                dataExcel.Dispose();
            }

            _fileResults.Add(fileCreated);
            excelMultipleTabs.Clear();
            await _context.AddAsync(new ApplicationLogTaskDetails
                { TaskId = _taskId, Step = "File created", Info = fName });
        }

        _fetchedData.Clear();
    }

    private async Task HandleTaskAlertAsync()
    {
        var headerParam = JsonSerializer.Deserialize<TaskHeaderParameters>(_header.TaskHeaderParameters, _jsonOpt);
        if (_fetchedData.Any())
        {
            var sendEmail = false;
            var a = _fetchedData.Select(a => a.Key).FirstOrDefault();
            if ((!headerParam!.AlertOccurenceByTime &&
                 a!.NbrOfCumulativeOccurences > headerParam.NbrOfOccurencesBeforeResendAlertEmail - 1) ||
                a!.NbrOfCumulativeOccurences == 0)
            {
                a.NbrOfCumulativeOccurences = 0;
                sendEmail = true;
            }

            if (headerParam.AlertOccurenceByTime & (a.LastRunDateTime <
                                                    DateTime.Now.AddMinutes(-headerParam
                                                        .NbrOfMinutesBeforeResendAlertEmail))) sendEmail = true;
            if (sendEmail || _jobParameters.ManualRun)
            {
                var emailPrefix = await _context.ApplicationParameters.Select(a => a.AlertEmailPrefix)
                    .FirstOrDefaultAsync();
                if (_header.TaskEmailRecipients.Select(a => a.Email).FirstOrDefault() != "[]")
                {
                    var subject = emailPrefix + " - " + a.TaskHeader?.ActivityName + ": " + a.TaskHeader?.TaskName;
                    var message = "";
                    List<Attachment> listAttach = new();
                    var counter = 0;
                    foreach (var table in _fetchedData.Where(keyValuePair => keyValuePair.Value.Rows.Count > 0))
                        if (table.Value.Rows.Count < 101)
                        {
                            var valueMessage = table.Key.QueryName + ":" + Environment.NewLine;
                            valueMessage += table.Value.ToHtml();
                            if (counter == 0)
                            {
                                message = string.Format(
                                    _header.TaskEmailRecipients.Select(a => a.Message).FirstOrDefault()!, valueMessage);
                                counter++;
                            }
                            else
                            {
                                message += "{0}";
                                message = string.Format(message, valueMessage);
                            }
                        }
                        else
                        {
                            ExcelCreationDatatable dataExcel = new()
                                { TabName = a.TaskHeader?.TaskName, Data = table.Value };
                            var fileResult = CreateFile.ExcelFromDatable(a.TaskHeader?.TaskName, dataExcel);
                            var fName =
                                $"{_header.ActivityName.RemoveSpecialExceptSpaceCharacters()}-{table.Key.QueryName.RemoveSpecialExceptSpaceCharacters()}_{DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx"}";
                            listAttach.Add(new Attachment(new MemoryStream(fileResult.FileContents), fName,
                                fileResult.ContentType));
                        }

                    var result = await _emailSender.SendEmailAsync(_emails, subject, message, listAttach);
                    if (result.Success)
                        await _context.AddAsync(new ApplicationLogTaskDetails
                            { TaskId = _taskId, Step = "Email sent", Info = subject });
                    else
                        await _context.AddAsync(new ApplicationLogTaskDetails
                            { TaskId = _taskId, Step = "Email not sent", Info = result.Message });

                    listAttach.Clear();
                }

                _header.TaskDetails.ToList().ForEach(a => a.LastRunDateTime = DateTime.Now);
            }

            a.NbrOfCumulativeOccurences++;
            //header.TaskDetails.ToList().ForEach(a => a.NbrOFCumulativeOccurences = nbrOccurrences);
            _context.Entry(a).State = EntityState.Modified;
        }
        else
        {
            var a = _header.TaskDetails.FirstOrDefault();
            a!.NbrOfCumulativeOccurences = 0;
            _context.Entry(a).State = EntityState.Modified;
        }
    }

    private async Task GenerateEmail()
    {
        if (_emails!.Any() && _fileResults.Any())
        {
            var emailPrefix = await _context.ApplicationParameters.Select(a => a.EmailPrefix).FirstOrDefaultAsync();
            var subject = emailPrefix + " - " + _header.ActivityName + ": " + _header.TaskName;

            List<Attachment> listAttach = new();
            listAttach.AddRange(_fileResults.Select(a =>
                new Attachment(new MemoryStream(a.FileContents), a.FileDownloadName, a.ContentType)).ToList());

            var message = _header.TaskEmailRecipients.Select(a => a.Message).FirstOrDefault();
            if (listAttach.Any())
                if (message != null)
                {
                    var result = await _emailSender.SendEmailAsync(_emails, subject, message, listAttach);
                    if (result.Success)
                        await _context.AddAsync(new ApplicationLogTaskDetails
                            { TaskId = _taskId, Step = "Email sent", Info = subject });
                    else
                        await _context.AddAsync(new ApplicationLogTaskDetails
                            { TaskId = _taskId, Step = "Email not sent", Info = result.Message });
                }
        }
    }
}