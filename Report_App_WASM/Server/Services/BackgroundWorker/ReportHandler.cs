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

                List<Attachment> listAttach = _fileResults.Select(a =>
                    new Attachment(new MemoryStream(a.Content), a.FileName, a.ContentType)).ToList();

                var message = _header.DistributionLists.Select(a => a.EmailMessage).FirstOrDefault();
                if (listAttach.Any() && message != null)
                {
                    var result = await _emailSender.SendEmailAsync(_emails, subject, message, listAttach);
                    await LogEmailResult(result, subject);
                }
            }
        }

        private async Task LogEmailResult(SubmitResult result, string subject)
        {
            if (result.Success)
            {
                await _context.AddAsync(new TaskStepLog
                {
                    TaskLogId = _taskId,
                    Step = "Email sent",
                    Info = subject,
                    RelatedLogType = LogType.EmailLog,
                    RelatedLogId = result.KeyValue
                });
            }
            else
            {
                await _context.AddAsync(new TaskStepLog
                {
                    TaskLogId = _taskId,
                    Step = "Email not sent",
                    Info = result.Message,
                    Error = true
                });
            }
        }

        private async ValueTask WriteFileAsync(MemoryFileContainer fileResult, string fName, bool useDepositConfiguration, string? subName = null)
        {
            var _fileLength= fileResult.Content.Length;
            var localFileResult = await _fileDeposit.SaveFileForBackupAsync(fileResult, fName);
            if (!localFileResult.Success)
                await _emailSender.GenerateErrorEmailAsync(localFileResult.Message, "Local file writing: ");

            await LogFileCreation(localFileResult, fName,_fileLength);

            if (_header.FileStorageLocationId != 0 && useDepositConfiguration && localFileResult.Success)
            {
                await HandleRemoteFileStorage(fileResult, fName, localFileResult);
            }
        }

        private async Task LogFileCreation(SubmitResult localFileResult, string fName, int _fileLength)
        {
            var filecreationLocal = new ReportGenerationLog
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
                FileSizeInMb = BytesConverter.ConvertBytesToMegabytes(_fileLength)
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
        }

        private async Task HandleRemoteFileStorage(MemoryFileContainer fileResult, string fName, SubmitResult localFileResult)
        {
            var config = await _context.FileStorageLocation.Include(a => a.SftpConfiguration).AsNoTracking()
                .FirstAsync(a => a.FileStorageLocationId == _header.FileStorageLocationId);

            var filecreationRemote = new ReportGenerationLog
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
                FileSizeInMb = BytesConverter.ConvertBytesToMegabytes(fileResult.Content.Length)
            };
            await _context.AddAsync(filecreationRemote);
            await _context.SaveChangesAsync("backgroundworker");

            var resultDeposit = await UploadFileToRemoteStorage(fileResult, fName, config);
            await LogRemoteFileStorageResult(resultDeposit, filecreationRemote, config.FilePath);
        }

        private async Task<SubmitResult> UploadFileToRemoteStorage(MemoryFileContainer fileResult, string fName, FileStorageLocation config)
        {
            string completePath;
            SubmitResult resultDeposit;
            var storagePath = Path.Combine(_hostingEnvironment.WebRootPath, "docsstorage");
            var localfilePath = Path.Combine(storagePath, fName);

            if (config.SftpConfiguration.UseFtpProtocol)
            {
                using var ftp = new FtpService(_context);
                resultDeposit = await ftp.UploadFileAsync(config.SftpConfiguration.SftpConfigurationId,
                    localfilePath, config.FilePath, fName, config.TryToCreateFolder);
            }
            else
            {
                using var sftp = new SftpService(_context);
                resultDeposit = await sftp.UploadFileAsync(config.SftpConfiguration.SftpConfigurationId,
                    localfilePath, config.FilePath, fName, config.TryToCreateFolder);
            }

            return resultDeposit;
        }

        private async Task LogRemoteFileStorageResult(SubmitResult resultDeposit, ReportGenerationLog filecreationRemote, string filePath)
        {
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

            filecreationRemote.ReportPath = filePath;
            filecreationRemote.Error = !resultDeposit.Success;
            filecreationRemote.Result = resultDeposit.Message;

            _context.Update(filecreationRemote);
            await _context.SaveChangesAsync("backgroundworker");
        }

        private async ValueTask GenerateFile()
        {
            var headerParam = JsonSerializer.Deserialize<ScheduledTaskParameters>(_header.TaskParameters, _jsonOpt);
            var informationSheet = false;
            List<ExcelCreationDatatable> excelMultipleTabs = new();

            foreach (var d in _fetchedData)
            {
                var detailParam = JsonSerializer.Deserialize<ScheduledTaskQueryParameters>(d.Key.ExecutionParameters!, _jsonOpt);
                var fName = GenerateFileName(detailParam, d.Key.QueryName);
                var fileCreated = await CreateFileBasedOnType(d, detailParam, headerParam, fName,  informationSheet, excelMultipleTabs);

                if (fileCreated != null)
                {
                    _fileResults.Add(fileCreated);
                    await _context.AddAsync(new TaskStepLog { TaskLogId = _taskId, Step = "File created", Info = fName });
                }
            }

            if (excelMultipleTabs.Any())
            {
                await CreateExcelFileWithMultipleTabs(headerParam, informationSheet, excelMultipleTabs);
            }

            _fetchedData.Clear();
        }

        private string GenerateFileName(ScheduledTaskQueryParameters? detailParam, string queryName)
        {
            return string.IsNullOrEmpty(detailParam?.FileName)
                ? $"{_header.ProviderName.RemoveSpecialExceptSpaceCharacters()}-{queryName.RemoveSpecialExceptSpaceCharacters()}_{DateTime.Now:yyyyMMdd_HHmmss}"
                : $"{detailParam.FileName.RemoveSpecialExceptSpaceCharacters()}_{DateTime.Now:yyyyMMdd_HHmmss}";
        }

        private async Task<MemoryFileContainer?> CreateFileBasedOnType(KeyValuePair<ScheduledTaskQuery, DataTable> d, ScheduledTaskQueryParameters? detailParam, ScheduledTaskParameters? headerParam, string fName,  bool informationSheet, List<ExcelCreationDatatable> excelMultipleTabs)
        {
            MemoryFileContainer? fileCreated = null;
            string fExtension;

            switch (_header.TypeFile)
            {
                case FileType.Excel:
                    fileCreated = await CreateExcelFile(d, detailParam, headerParam, fName,  informationSheet, excelMultipleTabs);
                    break;
                case FileType.Json:
                    fExtension = ".json";
                    fName += fExtension;
                    fileCreated = CreateFile.JsonFromDatable(fName, d.Value, detailParam!.EncodingType ?? "UTF8");
                    break;
                case FileType.Csv:
                    fExtension = ".csv";
                    fName += fExtension;
                    fileCreated = CreateFile.CsvFromDatable(fName, d.Value, detailParam!.EncodingType, headerParam?.Delimiter, detailParam.RemoveHeader);
                    break;
                case FileType.Xml:
                    fExtension = ".xml";
                    fName += fExtension;
                    fileCreated = CreateFile.XmlFromDatable(d.Key.QueryName, fName, detailParam?.EncodingType, d.Value);
                    break;
            }

            return fileCreated;
        }

        private async Task<MemoryFileContainer?> CreateExcelFile(KeyValuePair<ScheduledTaskQuery, DataTable> d, ScheduledTaskQueryParameters? detailParam, ScheduledTaskParameters? headerParam, string fName,  bool informationSheet, List<ExcelCreationDatatable> excelMultipleTabs)
        {
            MemoryFileContainer? fileCreated = null;
            string fExtension = ".xlsx";
            fName += fExtension;

            if (detailParam!.SeparateExcelFile)
            {
                List<ExcelCreationDatatable> excelCreationDatatables = new()
                    {
                        new ExcelCreationDatatable { TabName = detailParam.ExcelTabName ?? d.Key.QueryName, Data = d.Value }
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
                        throw new NullReferenceException($"Tabname of excel template for query {d.Key.QueryName} is null.");
                    tabName = template.ExcelTabName;
                }
                else
                {
                    tabName = detailParam.ExcelTabName ?? d.Key.QueryName;
                }

                excelMultipleTabs.Add(new ExcelCreationDatatable { TabName = tabName, ExcelTemplate = template, Data = d.Value });
            }

            return fileCreated;
        }

        private async Task CreateExcelFileWithMultipleTabs(ScheduledTaskParameters? headerParam, bool informationSheet, List<ExcelCreationDatatable> excelMultipleTabs)
        {
            var fName = string.IsNullOrEmpty(headerParam?.ExcelFileName)
                ? $"{_header.ProviderName.RemoveSpecialExceptSpaceCharacters()}-{_header.TaskName.RemoveSpecialExceptSpaceCharacters()}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                : $"{headerParam.ExcelFileName.RemoveSpecialExceptSpaceCharacters()}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

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
            await _context.AddAsync(new TaskStepLog { TaskLogId = _taskId, Step = "File created", Info = fName });
        }
    }
}
