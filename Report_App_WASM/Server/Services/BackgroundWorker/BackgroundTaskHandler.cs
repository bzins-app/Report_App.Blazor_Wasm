using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_App_BlazorServ.Services.RemoteDb;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Utils;
using Report_App_WASM.Server.Utils.RemoteQueryParameters;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.Extensions;
using Report_App_WASM.Shared.RemoteQueryParameters;
using Report_App_WASM.Shared.SerializedParameters;
using ReportAppWASM.Server.Services.EmailSender;
using ReportAppWASM.Server.Services.FilesManagement;
using System.Data;
using System.Net.Mail;
using System.Text.Json;

namespace ReportAppWASM.Server.Services.BackgroundWorker
{
    public class BackgroundTaskHandler : IDisposable
    {
        readonly ApplicationDbContext _context;
        readonly IEmailSender _emailSender;
        readonly IRemoteDbConnection _dbReader;
        readonly LocalFilesService _fileDeposit;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public BackgroundTaskHandler(
                      ApplicationDbContext context, IEmailSender emailSender, IRemoteDbConnection dbReader, LocalFilesService fileDeposit, IMapper mapper, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _emailSender = emailSender;
            _dbReader = dbReader;
            _fileDeposit = fileDeposit;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
        }

        private Dictionary<TaskDetail, DataTable> fetchedData = new();
        private List<FileContentResult> fileResults = new();
        private List<EmailRecipient> emails = new();
        private TaskJobParameters jobParameters;
        private TaskHeader header;
        private int taskId;

        public async Task HandleTask(TaskJobParameters parameters)
        {
            jobParameters = parameters;
            header = await _context.TaskHeader.Where(a => a.TaskHeaderId == parameters.TaskHeaderId).Include(a => a.Activity).Include(a => a.TaskDetails).Include(a => a.TaskEmailRecipients).FirstOrDefaultAsync();
            ApplicationLogTask logTask = new() { ActivityId = header.Activity.ActivityId, ActivityName = header.ActivityName, StartDateTime = DateTime.Now, JobDescription = header.TaskName, Type = header.Type + " service", Result = "Ok", RunBy = jobParameters.RunBy , EndDateTime=DateTime.Now};

            if (!header.TaskDetails.Any())
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
            taskId = logTask.Id;
            await _context.AddAsync(new ApplicationLogTaskDetails { TaskId = taskId, Step = "Initialization", Info = "Nbr of queries:" + header.TaskDetails.Count });

            try
            {
                if (header.Type != TaskType.DataTransfer)
                {
                    if (header.SendByEmail && header.TaskEmailRecipients.Select(a => a.Email).FirstOrDefault() != "[]")
                    {
                        emails = JsonSerializer.Deserialize<List<EmailRecipient>>(header.TaskEmailRecipients.Select(a => a.Email).FirstOrDefault());
                    }
                    if (jobParameters.ManualRun)
                    {
                        emails = jobParameters.CustomEmails;
                    }
                    foreach (var detail in header.TaskDetails)
                    {
                        await FetchData(detail);
                        await _context.SaveChangesAsync("backgroundworker");
                    }
                    if (header.Type == TaskType.Alert)
                    {
                        await HandleTaskAlertAsync();
                    }
                    else
                    {
                        await GenerateFile();
                        foreach (var f in fileResults)
                        {
                            await WriteFileAsync(f, f.FileDownloadName, jobParameters.GenerateFiles, f.FileDownloadName);
                        }
                        await GenerateEmail();
                        fileResults.Clear();
                    }

                    fetchedData.Clear();
                }
                else
                {
                    foreach (var detail in header.TaskDetails)
                    {
                        ApplicationLogTask log = new() { ActivityId = header.Activity.ActivityId, ActivityName = header.ActivityName, StartDateTime = DateTime.Now, JobDescription = detail.QueryName, Type = header.Type + " service", Error = false, RunBy = jobParameters.RunBy };

                        await FetchData(detail);
                        await _context.SaveChangesAsync("backgroundworker");

                        foreach (var value in fetchedData)
                        {
                            await HandleDataTransferTask(detail, value.Value, log);
                        }

                        log.EndDateTime = DateTime.Now;
                        log.DurationInSeconds = (int)(log.EndDateTime - log.StartDateTime).TotalSeconds;
                        await _context.AddAsync(log);
                        await _context.SaveChangesAsync("backgroundworker");
                        fetchedData.Clear();
                    }
                }

                logTask.Error = false;
                logTask.EndDateTime = DateTime.Now;
                logTask.DurationInSeconds = (int)(logTask.EndDateTime - logTask.StartDateTime).TotalSeconds;
                if (parameters.GenerateFiles)
                {
                    header.LastRunDateTime = DateTime.Now;
                    _context.Entry(header).State = EntityState.Modified;
                }
                await _context.AddAsync(new ApplicationLogTaskDetails { TaskId = taskId, Step = "Job end", Info = $"Total duration {logTask.DurationInSeconds} seconds" });
            }
            catch (Exception ex)
            {
                logTask.Result = new string(ex.Message.Take(2000).ToArray());
                logTask.Error = true;
                logTask.EndDateTime = DateTime.Now;
                logTask.DurationInSeconds = (int)(logTask.EndDateTime - logTask.StartDateTime).TotalSeconds;
                await _emailSender.GenerateErrorEmailAsync(ex.Message, header.ActivityName + ": " + header.TaskName);
                await _context.AddAsync(new ApplicationLogTaskDetails { TaskId = taskId, Step = "Error", Info = logTask.Result });
            }

            _context.Update(logTask);
            await _context.SaveChangesAsync("backgroundworker");
        }


        private async Task FetchData(TaskDetail detail)
        {
            using (var remoteDb = new RemoteDbConnection(_context, _mapper))
            {
                var detailParam = JsonSerializer.Deserialize<TaskDetailParameters>(detail.TaskDetailParameters);
                List<QueryCommandParameter> param = new();
                if (jobParameters.CustomQueryParameters.Any())
                {
                    param = jobParameters.CustomQueryParameters;
                }
                else
                if (header.UseGlobalQueryParameters && header.QueryParameters != "[]" &&
                    !string.IsNullOrEmpty(header.QueryParameters))
                {
                    param = JsonSerializer.Deserialize<List<QueryCommandParameter>>(header.QueryParameters);
                }

                if (detail.QueryParameters != "[]" && !string.IsNullOrEmpty(detail.QueryParameters))
                {
                    var desParam = JsonSerializer.Deserialize<List<QueryCommandParameter>>(detail.QueryParameters);
                    foreach (var value in desParam)
                    {
                        if (!param.Any(a => a.ParameterIdentifier.ToLower() == value.ParameterIdentifier.ToLower()))
                        {
                            param.Add(value);
                        }
                    }
                }
                DataTable table = await remoteDb.RemoteDbToDatableAsync(new RemoteDbCommandParameters() { ActivityId = header.Activity.ActivityId, QueryToRun = detail.Query, QueryInfo = detail.QueryName, PaginatedResult = true, LastRunDateTime = detail.LastRunDateTime ?? DateTime.Now, QueryCommandParameters = param }, jobParameters.Cts, taskId);
                await _context.AddAsync(new ApplicationLogTaskDetails { TaskId = taskId, Step = "Fetch data completed", Info = detail.QueryName + "- Nbr of rows:" + table.Rows.Count });

                if (detailParam.GenerateIfEmpty || table.Rows.Count > 0)
                {
                    fetchedData.Add(detail, table);
                }

                if (jobParameters.GenerateFiles)
                {
                    detail.LastRunDateTime = DateTime.Now;
                    _context.Entry(detail).State = EntityState.Modified;
                }

            };
        }
        private async Task WriteFileAsync(FileContentResult fileResult, string fName, bool useDepositConfiguration, string subName = null)
        {
            var localFileResult = await _fileDeposit.SaveFileForBackupAsync(fileResult, fName);
            if (!localFileResult.Success)
            {
                await _emailSender.GenerateErrorEmailAsync(localFileResult.Message, "Local file writing: ");
            }

            ApplicationLogReportResult filecreationLocal = new()
            {
                ActivityId = header.Activity.ActivityId,
                ActivityName = header.ActivityName,
                CreatedAt = DateTime.Now,
                CreatedBy = "Report Service",
                TaskHeaderId = header.TaskHeaderId,
                ReportName = header.TaskName,
                SubName = subName ?? "",
                FileType = header.TypeFile.ToString(),
                ReportPath = "/docsstorage/" + fName,
                FileName = fName,
                IsAvailable = true,
                Error = !localFileResult.Success,
                Result = localFileResult.Message,
                FileSizeInMB = BytesConverter.ConvertBytesToMegabytes(fileResult.FileContents.Length)
            };
            await _context.AddAsync(filecreationLocal);
            await _context.AddAsync(new ApplicationLogTaskDetails { TaskId = taskId, Step = "File local storage", Info = "Ok" });

            if (header.FileDepositPathConfigurationId != 0 && useDepositConfiguration && localFileResult.Success)
            {
                string completePath = "";
                SubmitResult resultDeposit;
                var config = await _context.FileDepositPathConfiguration.Include(a => a.SFTPConfiguration).AsNoTracking().FirstAsync(a => a.FileDepositPathConfigurationId == header.FileDepositPathConfigurationId);
                if (config.SFTPConfiguration != null && config.UseSFTPProtocol)
                {
                    var StoragePath = Path.Combine(_hostingEnvironment.WebRootPath, "docsstorage");
                    var localfilePath = Path.Combine(StoragePath, fName);
                    if (config.SFTPConfiguration.UseFTPProtocol)
                    {
                        completePath = "FTP Host:" + config.SFTPConfiguration.Host + " Path:" + config.FilePath;
                        using var ftp = new FtpService(_context);
                        resultDeposit = await ftp.UploadFileAsync(config.SFTPConfiguration.SFTPConfigurationId, localfilePath, config.FilePath, fName, config.TryToCreateFolder);
                        await _context.AddAsync(new ApplicationLogTaskDetails { TaskId = taskId, Step = "File FTP drop", Info = config.FilePath });
                    }
                    else
                    {
                        completePath = "sFTP Host:" + config.SFTPConfiguration.Host + " Path:" + config.FilePath;
                        using var sftp = new SftpService(_context);
                        resultDeposit = await sftp.UploadFileAsync(config.SFTPConfiguration.SFTPConfigurationId, localfilePath, config.FilePath, fName, config.TryToCreateFolder);
                        await _context.AddAsync(new ApplicationLogTaskDetails { TaskId = taskId, Step = "File sFTP drop", Info = config.FilePath });
                    }
                }
                else
                {
                    completePath = Path.Combine(config.FilePath, fName);
                    resultDeposit = await _fileDeposit.SaveFileAsync(fileResult, fName, config.FilePath, config.TryToCreateFolder);
                    await _context.AddAsync(new ApplicationLogTaskDetails { TaskId = taskId, Step = "File folder drop", Info = config.FilePath });
                }

                if (!resultDeposit.Success)
                {
                    await _emailSender.GenerateErrorEmailAsync(resultDeposit.Message, "File deposit: ");
                }

                ApplicationLogReportResult filecreationRemote = new()
                {
                    ActivityId = header.Activity.ActivityId,
                    ActivityName = header.ActivityName,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "Report Service",
                    TaskHeaderId = header.TaskHeaderId,
                    ReportName = header.TaskName,
                    SubName = subName ?? "",
                    FileType = header.TypeFile.ToString(),
                    ReportPath = completePath,
                    FileName = fName,
                    IsAvailable = false,
                    Error = !resultDeposit.Success,
                    Result = resultDeposit.Message,
                    FileSizeInMB = filecreationLocal.FileSizeInMB
                };
                await _context.AddAsync(filecreationRemote);
            }

        }
        private async Task HandleDataTransferTask(TaskDetail a, DataTable data, ApplicationLogTask logTask)
        {
            if (data.Rows.Count > 0)
            {
                var detailParam = JsonSerializer.Deserialize<TaskDetailParameters>(a.TaskDetailParameters);
                string checkTableQuery = $@"IF (EXISTS (SELECT *
                                                       FROM INFORMATION_SCHEMA.TABLES
                                                       WHERE TABLE_SCHEMA = 'dbo'
                                                       AND TABLE_NAME = '{detailParam.DataTransferTargetTableName}'))
                                                       BEGIN
                                                          select 1
                                                       END;
                                                    ELSE
                                                       BEGIN
                                                          select 0
                                                       END;";
                var result = await _dbReader.CkeckTableExists(checkTableQuery);

                if (!result)
                {
                    string queryCreate;
                    if (detailParam.DataTransferUsePK)
                    {
                        queryCreate = CreateSqlServerTableFromDatatable.CreateTableFromSchema(data, detailParam.DataTransferTargetTableName, false, detailParam.DataTransferPK);
                    }
                    else
                    {
                        if (detailParam.DataTransferCommandBehaviour == DataTransferBasicBehaviour.Append.ToString())
                        {
                            queryCreate = CreateSqlServerTableFromDatatable.CreateTableFromSchema(data, detailParam.DataTransferTargetTableName, false);
                        }
                        else
                        {
                            queryCreate = CreateSqlServerTableFromDatatable.CreateTableFromSchema(data, detailParam.DataTransferTargetTableName, true);
                        }

                    }
                    await _dbReader.CreateTable(queryCreate);
                }

                if (!detailParam.DataTransferUsePK)
                {
                    logTask.Result = "Lines inserted (command:" + detailParam.DataTransferCommandBehaviour + "): " + data.Rows.Count;
                    await _dbReader.LoadDatatableToTable(data, detailParam.DataTransferTargetTableName);
                    await _context.AddAsync(new ApplicationLogTaskDetails { TaskId = taskId, Step = "Bulk insert completed", Info = "Lines inserted (command:" + detailParam.DataTransferCommandBehaviour + "): " + data.Rows.Count });
                }
                else
                {
                    var tempTable = "tmp_" + detailParam.DataTransferTargetTableName + DateTime.Now.ToString("yyyyMMddHHmmss");
                    var columnNames = new HashSet<string>(data.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                    string queryCreate = CreateSqlServerTableFromDatatable.CreateTableFromSchema(data, tempTable, true, detailParam.DataTransferPK);

                    await _dbReader.CreateTable(queryCreate);
                    try
                    {
                        await _dbReader.LoadDatatableToTable(data, tempTable);
                        await _context.AddAsync(new ApplicationLogTaskDetails { TaskId = taskId, Step = "Bulk insert completed", Info = "Lines inserted (command:" + detailParam.DataTransferCommandBehaviour + "): " + data.Rows.Count });
                    }
                    catch (Exception)
                    {
                        await _dbReader.DeleteTable(tempTable);
                        throw;
                    }

                    var mergeSql = new
                    {
                        MERGE_FIELD_NAME = string.Join(" and ", detailParam.DataTransferPK.Select(name => $"target.[{name}] = source.[{name}]")),
                        FIELD_LIST = string.Join(", ", columnNames.Select(name => $"[{name}]")),
                        SOURCE_TABLE_NAME = tempTable,
                        TARGET_TABLE_NAME = detailParam.DataTransferTargetTableName,
                        UPDATES_LIST = string.Join(", ", columnNames.Select(name => $"target.[{name}] = source.[{name}]")),
                        SOURCE_FIELD_LIST = string.Join(", ", columnNames.Select(name => $"source.[{name}]")),
                        UPDATE_REQUIRED_EXPRESSION = string.Join(" OR ", columnNames.Select(name => $"IIF((target.[{name}] IS NULL AND source.[{name}] IS NULL) OR target.[{name}] = source.[{name}], 1, 0) = 0")) // Take care around null values
                    };

                    string MergeSqlTemplate;

                    if (detailParam.DataTransferCommandBehaviour == DataTransferAdvancedBehaviour.Insert.ToString())
                    {
                        MergeSqlTemplate = @$"DECLARE @SummaryOfChanges TABLE(Change VARCHAR(20));
                                             MERGE INTO {mergeSql.TARGET_TABLE_NAME} WITH (HOLDLOCK) AS target
                                             USING (SELECT * FROM {mergeSql.SOURCE_TABLE_NAME}) as source
                                             ON ({mergeSql.MERGE_FIELD_NAME})
                                             WHEN NOT MATCHED THEN
                                                 INSERT ({mergeSql.FIELD_LIST}) VALUES ({mergeSql.SOURCE_FIELD_LIST})
                                             OUTPUT $action INTO @SummaryOfChanges;

                                             SELECT Change, COUNT(1) AS CountPerChange
                                             FROM @SummaryOfChanges
                                             GROUP BY Change;";
                    }
                    else if (detailParam.DataTransferCommandBehaviour == DataTransferAdvancedBehaviour.InsertOrUpdateOrDelete.ToString())
                    {
                        MergeSqlTemplate = @$"DECLARE @SummaryOfChanges TABLE(Change VARCHAR(20));
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
                    }
                    else
                    {
                        MergeSqlTemplate = @$"DECLARE @SummaryOfChanges TABLE(Change VARCHAR(20));
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
                    }

                    var mergeResult = await _dbReader.MergeTables(MergeSqlTemplate);

                    logTask.Result = "Nbr lines inserted: " + mergeResult.InsertedCount + " Nbr lines updated: " + mergeResult.UpdatedCount + " Nbr lines deleted: " + mergeResult.DeletedCount;
                    await _dbReader.DeleteTable(tempTable);
                    await _context.AddAsync(new ApplicationLogTaskDetails { TaskId = taskId, Step = "Merge completed", Info = "Nbr lines inserted: " + mergeResult.InsertedCount + " Nbr lines updated: " + mergeResult.UpdatedCount + " Nbr lines deleted: " + mergeResult.DeletedCount });
                }
            }
        }
        private async Task GenerateFile()
        {
            string fName;
            string fExtension;
            bool informationSheet = false;
            List<ExcelCreationDatatable> excelMultipleTabs = new();
            var headerParam = JsonSerializer.Deserialize<TaskHeaderParameters>(header.TaskHeaderParameters);

            foreach (var d in fetchedData)
            {
                FileContentResult fileCreated;
                var detailParam = JsonSerializer.Deserialize<TaskDetailParameters>(d.Key.TaskDetailParameters);
                if (string.IsNullOrEmpty(detailParam.FileName))
                {
                    fName = $"{header.ActivityName.RemoveSpecialExceptSpaceCharacters()}-{d.Key.QueryName.RemoveSpecialExceptSpaceCharacters()}_{DateTime.Now.ToString("yyyyMMdd_HH_mm_ss")}";
                }
                else
                {
                    fName = $"{detailParam.FileName.RemoveSpecialExceptSpaceCharacters()}_{DateTime.Now:yyyyMMdd_HH_mm_ss}";
                }

                if (header.TypeFile == FileType.Excel)
                {
                    if (detailParam.SeparateExcelFile)
                    {
                        fExtension = ".xlsx";
                        fName += fExtension;
                        List<ExcelCreationDatatable> excelCreationDatatables = new()
                        {
                            new ExcelCreationDatatable() { TabName = detailParam.ExcelTabName ?? d.Key.QueryName, Data = d.Value }
                        };
                        ExcelCreationData dataExcel = new() { FileName = fName, Data = excelCreationDatatables, ValidationSheet = detailParam.AddValidationSheet, ValidationText = headerParam.ValidationSheetText };

                        fileCreated = CreateFile.ExcelFromSeveralsDatable(dataExcel);
                        dataExcel.Dispose();
                    }
                    else
                    {
                        string tabName;
                        ExcelTemplate template = new();
                        if (!informationSheet)
                        {
                            informationSheet = detailParam.AddValidationSheet;
                        }
                        if (headerParam.UseAnExcelTemplate)
                        {
                            template = detailParam.ExcelTemplate;
                            if (string.IsNullOrEmpty(template.ExcelTabName))
                            {
                                throw new NullReferenceException($"Tabname of excel template for query {d.Key.QueryName} is null.");
                            }
                            tabName = template.ExcelTabName;
                        }
                        else
                        {
                            tabName = detailParam.ExcelTabName ?? d.Key.QueryName;
                        }
                        excelMultipleTabs.Add(new ExcelCreationDatatable() { TabName = tabName, ExcelTemplate = template, Data = d.Value });
                        continue;
                    }
                }
                else if (header.TypeFile == FileType.Json)
                {
                    fExtension = ".json";
                    fName += fExtension;
                    fileCreated = CreateFile.JsonFromDatable(fName, d.Value, detailParam.EncodingType ?? "UTF8");
                }
                else if (header.TypeFile == FileType.CSV)
                {
                    fExtension = ".csv";
                    fName += fExtension;
                    fileCreated = CreateFile.CsvFromDatable(fName, d.Value, detailParam.EncodingType ?? "UTF8", headerParam.Delimiter, detailParam.RemoveHeader);
                }
                else
                {
                    fExtension = ".xml";
                    fName += fExtension;
                    fileCreated = CreateFile.XMLFromDatable(d.Key.QueryName, fName, detailParam.EncodingType ?? "UTF8", d.Value);
                }
                fileResults.Add(fileCreated);
                await _context.AddAsync(new ApplicationLogTaskDetails { TaskId = taskId, Step = "File created", Info = fName });
            }
            if (excelMultipleTabs.Any())
            {
                fName = string.IsNullOrEmpty(headerParam.ExcelFileName) ? $"{header.ActivityName.RemoveSpecialExceptSpaceCharacters()}-{header.TaskName.RemoveSpecialExceptSpaceCharacters()}_{DateTime.Now.ToString("yyyyMMdd_HH_mm_ss") + ".xlsx"}" : $"{headerParam.ExcelFileName.RemoveSpecialExceptSpaceCharacters()}_{DateTime.Now.ToString("yyyyMMdd_HH_mm_ss") + ".xlsx"}";

                FileContentResult fileCreated;
                if (!headerParam.UseAnExcelTemplate)
                {
                    ExcelCreationData dataExcel = new() { FileName = fName, Data = excelMultipleTabs, ValidationSheet = informationSheet, ValidationText = headerParam.ValidationSheetText };

                    fileCreated = CreateFile.ExcelFromSeveralsDatable(dataExcel);
                    dataExcel.Dispose();
                }
                else
                {
                    ExcelCreationData dataExcel = new() { FileName = fName, Data = excelMultipleTabs, ValidationSheet = informationSheet, ValidationText = headerParam.ValidationSheetText };
                    var fileInfo = _fileDeposit.GetFileInfo(headerParam.ExcelTemplatePath);
                    fileCreated = CreateFile.ExcelTemplateFromSeveralDatable(dataExcel, fileInfo);
                    dataExcel.Dispose();
                }
                fileResults.Add(fileCreated);
                excelMultipleTabs.Clear();
                await _context.AddAsync(new ApplicationLogTaskDetails { TaskId = taskId, Step = "File created", Info = fName });
            }
            fetchedData.Clear();
        }
        private async Task HandleTaskAlertAsync()
        {
            var headerParam = JsonSerializer.Deserialize<TaskHeaderParameters>(header.TaskHeaderParameters);
            if (fetchedData.Any())
            {
                bool sendEmail = false;
                var a = fetchedData.Select(a => a.Key).FirstOrDefault();
                if (!headerParam.AlertOccurenceByTime && a.NbrOFCumulativeOccurences > headerParam.NbrOfOccurencesBeforeResendAlertEmail - 1 || a.NbrOFCumulativeOccurences == 0)
                {
                    a.NbrOFCumulativeOccurences = 0;
                    sendEmail = true;
                }
                if (headerParam.AlertOccurenceByTime & a.LastRunDateTime < DateTime.Now.AddMinutes(-headerParam.NbrOFMinutesBeforeResendAlertEmail))
                {
                    sendEmail = true;
                }
                if (sendEmail || jobParameters.ManualRun)
                {
                    var emailPrefix = await _context.ApplicationParameters.Select(a => a.AlertEmailPrefix).FirstOrDefaultAsync();
                    if ((header.SendByEmail && header.TaskEmailRecipients.Select(a => a.Email).FirstOrDefault() != "[]"))
                    {
                        string subject = emailPrefix + " - " + a.TaskHeader.ActivityName + ": " + a.TaskHeader.TaskName;
                        string preMessage = a.TaskHeader.TaskEmailRecipients.Select(a => a.Message).FirstOrDefault();
                        string message = "";
                        List<Attachment> listAttach = new();
                        int counter = 0;
                        foreach (var table in fetchedData.Where(a => a.Value.Rows.Count > 0))
                        {
                            if (table.Value.Rows.Count < 101)
                            {
                                var valueMessage = table.Key.QueryName + ":" + Environment.NewLine;
                                valueMessage += table.Value.ToHtml();
                                if (counter == 0)
                                {
                                    message = string.Format(header.TaskEmailRecipients.Select(a => a.Message).FirstOrDefault(), valueMessage);
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
                                ExcelCreationDatatable dataExcel = new() { TabName = a.TaskHeader.TaskName, Data = table.Value };
                                var fileResult = CreateFile.ExcelFromDatable(a.TaskHeader.TaskName, dataExcel);
                                var fName =
                                    $"{header.ActivityName.RemoveSpecialExceptSpaceCharacters()}-{table.Key.QueryName.RemoveSpecialExceptSpaceCharacters()}_{DateTime.Now.ToString("yyyyMMdd_HH_mm_ss") + ".xlsx"}";
                                listAttach.Add(new Attachment(new MemoryStream(fileResult.FileContents), fName, fileResult.ContentType));
                            }
                        }

                        await _emailSender.SendEmailAsync(emails, subject, message, listAttach);
                        await _context.AddAsync(new ApplicationLogTaskDetails { TaskId = taskId, Step = "Email sent", Info = subject });
                        listAttach.Clear();
                    }

                    header.TaskDetails.ToList().ForEach(a => a.LastRunDateTime = DateTime.Now);
                }
                var nbrOccurrences = a.NbrOFCumulativeOccurences++;
                //header.TaskDetails.ToList().ForEach(a => a.NbrOFCumulativeOccurences = nbrOccurrences);
                _context.Entry(a).State = EntityState.Modified;
            }
            else
            {
                var a = header.TaskDetails.FirstOrDefault();
                a.NbrOFCumulativeOccurences = 0;
                _context.Entry(a).State = EntityState.Modified;
            }

        }
        private async Task GenerateEmail()
        {
            if (emails.Any() && fileResults.Any())
            {
                var emailPrefix = await _context.ApplicationParameters.Select(a => a.EmailPrefix).FirstOrDefaultAsync();
                string subject = emailPrefix + " - " + header.ActivityName + ": " + header.TaskName;

                List<Attachment> listAttach = new();
                listAttach.AddRange(fileResults.Select(a => new Attachment(new MemoryStream(a.FileContents), a.FileDownloadName, a.ContentType)).ToList());

                //to remove dirty code
                if (header.TaskName.StartsWith("#"))
                {
                    subject = header.TaskName;
                }
                //
                string message = header.TaskEmailRecipients.Select(a => a.Message).FirstOrDefault();
                if (listAttach.Any())
                {
                    await _emailSender.SendEmailAsync(emails, subject, message, listAttach);
                    await _context.AddAsync(new ApplicationLogTaskDetails { TaskId = taskId, Step = "Email sent", Info = subject });
                }
            }
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.ReRegisterForFinalize(this);
        }
    }
}
