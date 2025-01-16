using System.Net.Mail;
using System.Text.Json;
using Report_App_WASM.Server.Services.EmailSender;
using Report_App_WASM.Server.Services.FilesManagement;
using Report_App_WASM.Server.Services.RemoteDb;
using Report_App_WASM.Server.Utils.BackgroundWorker;
using Report_App_WASM.Server.Utils.FIles;

namespace Report_App_WASM.Server.Services.BackgroundWorker
{
    public class ReportHandler : ScheduledTaskHandler
    {

        public ReportHandler(ApplicationDbContext context, IEmailSender emailSender,
            IRemoteDatabaseActionsHandler dbReader, LocalFilesService fileDeposit, IMapper mapper,
            IWebHostEnvironment hostingEnvironment) : base(context, emailSender, dbReader, fileDeposit, mapper, hostingEnvironment)
        {
            _context = context;
            _emailSender = emailSender;
            _dbReader = dbReader;
            _fileDeposit = fileDeposit;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
        }


        public async ValueTask HandleReportTask(TaskJobParameters parameters)
        {
            _jobParameters = parameters;
            _header = await GetScheduledTaskAsync(parameters.ScheduledTaskId);


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
                var _activityConnect = await GetDatabaseConnectionAsync(_header.IdDataProvider);
                var _resultInfo = "Ok";
                if (_header.SendByEmail && _header.DistributionLists.Select(a => a.Recipients).FirstOrDefault() != "[]")
                    _emails = JsonSerializer.Deserialize<List<EmailRecipient>>(_header.DistributionLists
                        .Select(a => a.Recipients).FirstOrDefault()!);
                if (_jobParameters.ManualRun) _emails = _jobParameters.CustomEmails;

                foreach (var detail in _header.TaskQueries.OrderBy(a => a.ExecutionOrder))
                {
                    await FetchData(detail, _activityConnect.TaskSchedulerMaxNbrofRowsFetched);
                    await _context.SaveChangesAsync("backgroundworker");
                }

                await GenerateFile();
                foreach (var f in _fileResults)
                {
                    await WriteFileAsync(f, f.FileName, _jobParameters.GenerateFiles, f.FileName);
                }

                await GenerateEmail();
                _fileResults.Clear();

                _fetchedData.Clear();

                await FinalizeTaskAsync(_logTask, parameters.GenerateFiles, _resultInfo);
            }
            catch (Exception ex)
            {
                await HandleTaskErrorAsync(_logTask, ex);
            }

            await UpdateTaskLogAsync(_logTask);
            await _context.SaveChangesAsync("backgroundworker");
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
                                TaskLogId = _taskId,
                                Step = "Email sent",
                                Info = subject,
                                RelatedLogType = LogType.EmailLog,
                                RelatedLogId = result.KeyValue
                            });
                        else
                            await _context.AddAsync(new TaskStepLog
                            { TaskLogId = _taskId, Step = "Email not sent", Info = result.Message, Error = true });
                    }
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
                Result = localFileResult.Message,
                FileGenerationType = FileGenerationType.LocalCopy,
                FileSizeInMb = BytesConverter.ConvertBytesToMegabytes(fileResult.Content.Length)
            };
            await _context.AddAsync(filecreationLocal);
            await _context.SaveChangesAsync("backgroundworker");
            _logTask.HasSteps = true;
            await _context.AddAsync(new TaskStepLog
            {
                TaskLogId = _taskId,
                Step = "File local storage",
                Info = "Ok",
                RelatedLogType = LogType.ReportGenerationLog,
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
                            TaskLogId = _taskId,
                            Step = "File FTP drop",
                            Info = config.FilePath,
                            RelatedLogType = LogType.ReportGenerationLog,
                            RelatedLogId = filecreationRemote.Id
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
                            TaskLogId = _taskId,
                            Step = "File Sftp drop",
                            Info = config.FilePath,
                            RelatedLogType = LogType.ReportGenerationLog,
                            RelatedLogId = filecreationRemote.Id
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
                        TaskLogId = _taskId,
                        Step = "File folder drop",
                        Info = config.FilePath,
                        RelatedLogType = LogType.ReportGenerationLog,
                        RelatedLogId = filecreationRemote.Id
                    });
                }

                if (!resultDeposit.Success)
                {
                    await _emailSender.GenerateErrorEmailAsync(resultDeposit.Message, "File deposit: ");
                    await _context.AddAsync(new TaskStepLog
                    {
                        TaskLogId = _taskId,
                        Step = "Error",
                        Info = resultDeposit.Message,
                        RelatedLogType = LogType.ReportGenerationLog,
                        RelatedLogId = filecreationRemote.Id,
                        Error = true
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

    }
}
