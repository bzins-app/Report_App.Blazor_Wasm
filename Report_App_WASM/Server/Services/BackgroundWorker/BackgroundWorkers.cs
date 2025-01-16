using System.Net.Mail;
using System.Text.Json;
using Hangfire;
using Hangfire.Storage;
using Report_App_WASM.Server.Services.EmailSender;
using Report_App_WASM.Server.Services.FilesManagement;
using Report_App_WASM.Server.Services.RemoteDb;
using Report_App_WASM.Server.Utils.BackgroundWorker;

namespace Report_App_WASM.Server.Services.BackgroundWorker;

public class BackgroundWorkers : IBackgroundWorkers, IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IRemoteDatabaseActionsHandler _dbReader;
    private readonly IEmailSender _emailSender;
    private readonly LocalFilesService _fileDeposit;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly IMapper _mapper;
    private readonly IServiceScopeFactory _scopeFactory;

    public BackgroundWorkers(
        ApplicationDbContext context, IEmailSender emailSender, IRemoteDatabaseActionsHandler dbReader,
        LocalFilesService fileDeposit, IMapper mapper, IWebHostEnvironment hostingEnvironment,
        IServiceScopeFactory scopeFactory)
    {
        _context = context;
        _emailSender = emailSender;
        _dbReader = dbReader;
        _fileDeposit = fileDeposit;
        _mapper = mapper;
        _hostingEnvironment = hostingEnvironment;
        _scopeFactory = scopeFactory;
    }


    public void SendEmail(List<EmailRecipient>? email, string? subject, string message,
        List<Attachment>? attachment = null)

    {
        BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(email, subject, message, attachment));
    }

    public void DeleteFile(string filePath)
    {
        BackgroundJob.Schedule(
            () => File.Delete(filePath), TimeSpan.FromMinutes(5));
    }

    public async Task SwitchBackgroundTasksPerActivityAsync(long dataProviderId, bool activate)
    {
        var reportHeaders = await _context.ScheduledTask
            .Where(a => a.IsEnabled && a.DataProvider.DataProviderId == dataProviderId)
            .Select(a => a.ScheduledTaskId).ToListAsync();

        foreach (var t in reportHeaders) await HandleTasksJobs(t, activate);
    }

    public async Task SwitchBackgroundTaskAsync(long taskHeaderId, bool activate)
    {
        await HandleTasksJobs(taskHeaderId, activate);
    }

    public async Task<SubmitResult> ActivateBackgroundWorkersAsync(bool activate, BackgroundTaskType type)
    {
        var queueName = type.ToString().ToLower();
        var options = new RecurringJobOptions { TimeZone = TimeZoneInfo.Local, QueueName = queueName };
        SubmitResult result = new();
        if (activate)
        {
            if (queueName == "cleaner")
            {
                RecurringJob.AddOrUpdate("CleanerJobFiles", queueName, () => DeleteLocalFilesAsync(), "1 0 * * *",
                    options);
                RecurringJob.AddOrUpdate("CleanerJobLogs", queueName, () => DeleteLogsAsync(), "3 0 * * *", options);
            }
            else
            {
                var typeTask = type switch
                {
                    BackgroundTaskType.DataTransfer => TaskType.DataTransfer,
                    BackgroundTaskType.Alert => TaskType.Alert,
                    _ => TaskType.Report
                };
                await _context.ScheduledTask
                    .Where(a => a.IsEnabled == true && a.Type == typeTask && a.DataProvider.IsEnabled)
                    .Include(scheduledTask => scheduledTask.DataProvider).ForEachAsync(
                        a =>
                        {
                            var jobName = a.TypeName + " Id:" + a.ScheduledTaskId;
                            options.TimeZone = string.IsNullOrEmpty(a.DataProvider.TimeZone)
                                ? TimeZoneInfo.Local
                                : TimeZoneInfo.FindSystemTimeZoneById(a.DataProvider.TimeZone);
                            if (!string.IsNullOrEmpty(a.CronParameters) || a.CronParameters != "[]")
                            {
                                var crons = JsonSerializer.Deserialize<List<CronParameters>>(a.CronParameters);
                                var cronId = 0;
                                foreach (var cron in crons!)
                                {
                                    var jobParam = new TaskJobParameters
                                    {
                                        ScheduledTaskId = a.ScheduledTaskId,
                                        Cts = CancellationToken.None,
                                        TaskType = a.Type,
                                        GenerateFiles = true
                                    };
                                    var jobId = jobName + "_" + cronId;
                                    RecurringJob.AddOrUpdate(jobId, queueName,
                                        () => RunTaskJobAsync(jobParam, CancellationToken.None),
                                        cron.CronValue, options);
                                    cronId++;
                                }
                            }
                        });
            }

            result.Success = true;
        }
        else
        {
            var recurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();
            foreach (var job in recurringJobs.Where(a => a.Queue == queueName)) RecurringJob.RemoveIfExists(job.Id);
            result.Success = false;
        }

        return result;
    }

    public void RunManuallyTask(long taskHeaderId, string? runBy, List<EmailRecipient>? emails,
        List<QueryCommandParameter> commandQueryParameters, bool generateFiles = false)
    {
        var _tType=  _context.ScheduledTask.Where(a => a.ScheduledTaskId == taskHeaderId)
            .Select(a => a.Type)
            .FirstOrDefault();
        BackgroundJob.Enqueue(() => RunTaskJobAsync(new TaskJobParameters
        {
            ScheduledTaskId = taskHeaderId,
            Cts = CancellationToken.None,
            GenerateFiles = generateFiles,
            TaskType = _tType,
            CustomEmails = emails ?? new List<EmailRecipient>(),
            QueryCommandParameters = commandQueryParameters,
            ManualRun = true,
            RunBy = runBy
        }, CancellationToken.None));
    }

    void IDisposable.Dispose()
    {
        GC.SuppressFinalize(this);
    }


    private async Task HandleTasksJobs(long taskHeaderId, bool activate)
    {
        var services = await _context.SystemServicesStatus
            .Select(a => new { a.AlertService, a.ReportService, a.DataTransferService })
            .FirstOrDefaultAsync();

        var taskHeader = await _context.ScheduledTask
            .AsNoTrackingWithIdentityResolution()
            .Where(a => a.ScheduledTaskId == taskHeaderId)
            .Select(a => new
            {
                a.TaskName,
                a.Type,
                a.TypeName,
                a.DataProvider.ProviderName,
                a.CronParameters,
                timeZone = string.IsNullOrEmpty(a.DataProvider.TimeZone)
                    ? TimeZoneInfo.Local.Id
                    : a.DataProvider.TimeZone
            })
            .FirstOrDefaultAsync();

        var jobName = taskHeader!.TypeName + " Id:" + taskHeaderId;

        if (activate)
        {
            var options = new RecurringJobOptions
            { TimeZone = TimeZoneInfo.FindSystemTimeZoneById(taskHeader.timeZone) };
            if (!string.IsNullOrEmpty(taskHeader.CronParameters) && taskHeader.CronParameters != "[]")
            {
                var crons = JsonSerializer.Deserialize<List<CronParameters>>(taskHeader.CronParameters);
                for (int cronId = 0; cronId < crons.Count; cronId++)
                {
                    var cron = crons[cronId];
                    var jobId = jobName + "_" + cronId;
                    var jobParam = new TaskJobParameters
                    {
                        ScheduledTaskId = taskHeaderId,
                        TaskType = taskHeader.Type,
                        Cts = CancellationToken.None,
                        GenerateFiles = true
                    };

                    if (taskHeader.Type == TaskType.Report && services!.ReportService)
                    {
                        var queueName = "report";
                        options.QueueName = queueName;
                        RecurringJob.AddOrUpdate(jobId, queueName,
                            () => RunTaskJobAsync(jobParam, CancellationToken.None), cron.CronValue, options);
                    }
                    else if (taskHeader.Type == TaskType.Alert && services!.AlertService)
                    {
                        var queueName = "alert";
                        options.QueueName = queueName;
                        RecurringJob.AddOrUpdate(jobId, queueName,
                            () => RunTaskJobAsync(jobParam, CancellationToken.None), cron.CronValue, options);
                    }
                    else if (taskHeader.Type == TaskType.DataTransfer && services!.DataTransferService)
                    {
                        var queueName = "datatransfer";
                        options.QueueName = queueName;
                        RecurringJob.AddOrUpdate(jobId, queueName,
                            () => RunTaskJobAsync(jobParam, CancellationToken.None), cron.CronValue, options);
                    }
                }
            }
        }
        else
        {
            var recurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();
            foreach (var j in recurringJobs.Where(a => a.Id.Contains(" Id:" + taskHeaderId + "_")))
                RecurringJob.RemoveIfExists(j.Id);
        }
    }

    public async ValueTask RunTaskJobAsync(TaskJobParameters parameters, CancellationToken cts)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetService<ApplicationDbContext>();
        parameters.Cts = cts;

        if (db != null)
        {
            if (parameters.TaskType == TaskType.DataTransfer)
            {
                using var handler = new DataTransferHandler(db, _emailSender, _dbReader, _fileDeposit, _mapper,
                    _hostingEnvironment);
                await handler.HandleDatatransferTask(parameters);
            }
            else if (parameters.TaskType == TaskType.Alert)
            {
                using var handler = new AlertHandler(db, _emailSender, _dbReader, _fileDeposit, _mapper,
                    _hostingEnvironment);
                await handler.HandleAlertTask(parameters);
            }
            else if (parameters.TaskType == TaskType.Report)
            {
                using var handler = new ReportHandler(db, _emailSender, _dbReader, _fileDeposit, _mapper,
                    _hostingEnvironment);
                await handler.HandleReportTask(parameters);
            }

        }
    }

    public async Task DeleteLocalFilesAsync()
    {
        TaskLog logTask = new()
        { StartDateTime = DateTime.Now, JobDescription = "File Cleaner", Type = "Cleaner service" };
        try
        {
            var qry = _context.ReportGenerationLog.Where(a => a.IsAvailable == true).GroupJoin(
                    _context.ScheduledTask,
                    rr => rr.ScheduledTaskId,
                    rh => rh.ScheduledTaskId,
                    (x, y) => new { rResults = x, rHeader = y })
                .SelectMany(
                    x => x.rHeader.DefaultIfEmpty(),
                    (x, y) => new { reportResult = x.rResults, taskHeader = y });

            var filesToDelete = await qry
                .Where(a => a.reportResult.CreatedAt.Date <
                            DateTime.Now.AddDays(-(a.taskHeader == null ? 90 : a.taskHeader.ReportsRetentionInDays)))
                .Select(a => a.reportResult).ToListAsync();

            if (filesToDelete.Any()) await _fileDeposit.RemoveLocalFilesAsync(filesToDelete);
            logTask.Result = "Ok";
            logTask.Error = false;
            logTask.EndDateTime = DateTime.Now;
            logTask.DurationInSeconds = (int)(logTask.EndDateTime - logTask.StartDateTime).TotalSeconds;
        }
        catch (Exception ex)
        {
            logTask.Result = ex.Message;
            logTask.Error = true;
            logTask.EndDateTime = DateTime.Now;
            logTask.DurationInSeconds = (int)(logTask.EndDateTime - logTask.StartDateTime).TotalSeconds;
            await _emailSender.GenerateErrorEmailAsync(ex.Message, "File deletion: ");
        }

        await _context.AddAsync(logTask);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteLogsAsync()
    {
        var logTask = new TaskLog
        {
            StartDateTime = DateTime.Now,
            JobDescription = "Logs cleaner",
            Type = "Cleaner service"
        };

        var retentionDays = await _context.SystemParameters
            .Select(a => a.LogsRetentionInDays)
            .FirstOrDefaultAsync();


        await _context.SystemLog
            .Where(a => a.TimeStamp.Date < DateTime.Today.AddDays(-retentionDays))
            .ForEachAsync(a => _context.Remove(a));
        await _context.SaveChangesAsync();
        await _context.TaskLog
            .Where(a => a.EndDateTime.Date < DateTime.Today.AddDays(-retentionDays))
            .ForEachAsync(a => _context.Remove(a));
        await _context.SaveChangesAsync();
        await _context.TaskStepLog
            .Where(a => a.TimeStamp.Date < DateTime.Today.AddDays(-retentionDays))
            .ForEachAsync(a => _context.Remove(a));
        await _context.SaveChangesAsync();
        await _context.EmailLog
            .Where(a => a.EndDateTime.Date < DateTime.Today.AddDays(-retentionDays))
            .ForEachAsync(a => _context.Remove(a));
        await _context.SaveChangesAsync();
        await _context.AuditTrail
            .Where(a => a.DateTime.Date < DateTime.Today.AddDays(-retentionDays))
            .ForEachAsync(a => _context.Remove(a));
        await _context.SaveChangesAsync();
        await _context.ReportGenerationLog
            .Where(a => a.CreatedAt.Date < DateTime.Today.AddDays(-retentionDays) && !a.IsAvailable)
            .ForEachAsync(a => _context.Remove(a));
        await _context.SaveChangesAsync();
        await _context.QueryExecutionLog
            .Where(a => a.StartDateTime.Date < DateTime.Today.AddDays(-retentionDays))
            .ForEachAsync(a => _context.Remove(a));
        await _context.SaveChangesAsync();
        await _context.AdHocQueryExecutionLog
            .Where(a => a.StartDateTime.Date < DateTime.Today.AddDays(-retentionDays))
            .ForEachAsync(a => _context.Remove(a));
        await _context.SaveChangesAsync();


        logTask.Result = "Ok";
        logTask.Error = false;
        logTask.EndDateTime = DateTime.Now;
        logTask.DurationInSeconds = (int)(logTask.EndDateTime - logTask.StartDateTime).TotalSeconds;
        await _context.AddAsync(logTask);
        await _context.SaveChangesAsync();
    }
}