using Report_App_WASM.Server.Services.EmailSender;
using Report_App_WASM.Server.Services.FilesManagement;
using Report_App_WASM.Server.Services.RemoteDb;
using Report_App_WASM.Server.Utils.FIles;
using System.Net.Mail;
using System.Text.Json;
using Report_App_WASM.Server.Utils.BackgroundWorker;

namespace Report_App_WASM.Server.Services.BackgroundWorker
{
    public class AlertHandler : ScheduledTaskHandler
    {


        public AlertHandler(ApplicationDbContext context, IEmailSender emailSender,
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


        public async ValueTask HandleAlertTask(TaskJobParameters parameters)
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

                await GenerateAlertAsync();
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


        private async ValueTask GenerateAlertAsync()
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
                                TaskLogId = _taskId,
                                Step = "Email sent",
                                Info = subject,
                                RelatedLogType = LogType.EmailLog,
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

    }
}
