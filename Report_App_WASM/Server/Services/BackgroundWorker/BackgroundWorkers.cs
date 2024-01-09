using System.Net.Mail;
using System.Text.Json;
using AutoMapper;
using Hangfire;
using Hangfire.Storage;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Services.EmailSender;
using Report_App_WASM.Server.Services.FilesManagement;
using Report_App_WASM.Server.Services.RemoteDb;
using Report_App_WASM.Server.Utils;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.SerializedParameters;

namespace Report_App_WASM.Server.Services.BackgroundWorker;

public class BackgroundWorkers : IBackgroundWorkers, IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IRemoteDbConnection _dbReader;
    private readonly IEmailSender _emailSender;
    private readonly LocalFilesService _fileDeposit;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly IMapper _mapper;
    private readonly IServiceScopeFactory _scopeFactory;

    public BackgroundWorkers(
        ApplicationDbContext context, IEmailSender emailSender, IRemoteDbConnection dbReader,
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

    public async Task SwitchBackgroundTasksPerActivityAsync(int activityId, bool activate)
    {
        var reportHeaders = await _context.TaskHeader.Where(a => a.IsActivated && a.Activity.ActivityId == activityId)
            .Select(a => a.TaskHeaderId).ToListAsync();

        foreach (var t in reportHeaders) await HandleTasksJobs(t, activate);
    }

    public async Task SwitchBackgroundTaskAsync(int taskHeaderId, bool activate)
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
                await _context.TaskHeader
                    .Where(a => a.IsActivated == true && a.Type == typeTask && a.Activity.IsActivated).ForEachAsync(
                        a =>
                        {
                            var jobName = a.TypeName + " Id:" + a.TaskHeaderId;
                            if (!string.IsNullOrEmpty(a.CronParameters) || a.CronParameters != "[]")
                            {
                                var crons = JsonSerializer.Deserialize<List<CronParameters>>(a.CronParameters);
                                var cronId = 0;
                                foreach (var cron in crons!)
                                {
                                    var jobParam = new TaskJobParameters
                                    {
                                        TaskHeaderId = a.TaskHeaderId,
                                        Cts = CancellationToken.None,
                                        GenerateFiles = true
                                    };
                                    var jobId = jobName + "_" + cronId;
                                    RecurringJob.AddOrUpdate(jobId, queueName, () => RunTaskJobAsync(jobParam, CancellationToken.None),
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

    public void RunManuallyTask(int taskHeaderId, string? runBy, List<EmailRecipient>? emails,
        List<QueryCommandParameter> customQueryParameters, bool generateFiles = false)
    {
        BackgroundJob.Enqueue(() => RunTaskJobAsync(new TaskJobParameters
        {
            TaskHeaderId = taskHeaderId, Cts = CancellationToken.None, GenerateFiles = generateFiles,
            CustomEmails = emails ?? new List<EmailRecipient>(), CustomQueryParameters = customQueryParameters,
            ManualRun = true, RunBy = runBy
        }, CancellationToken.None));
    }

    void IDisposable.Dispose()
    {
        GC.Collect();
        GC.SuppressFinalize(this);
    }

    private async Task HandleTasksJobs(int taskHeaderId, bool activate)
    {
        var services = await _context.ServicesStatus
            .Select(a => new { a.AlertService, a.ReportService, a.DataTransferService }).FirstOrDefaultAsync();
        var taskHeader = await _context.TaskHeader.AsNoTrackingWithIdentityResolution()
            .Where(a => a.TaskHeaderId == taskHeaderId)
            .Select(a => new { a.TaskName, a.Type, a.TypeName, a.Activity.ActivityName, a.CronParameters })
            .FirstOrDefaultAsync();
        var jobName = taskHeader!.TypeName + " Id:" + taskHeaderId;
        if (activate)
        {
            var options = new RecurringJobOptions { TimeZone = TimeZoneInfo.Local };
            if (!string.IsNullOrEmpty(taskHeader.CronParameters) || taskHeader.CronParameters != "[]")
            {
                var crons = JsonSerializer.Deserialize<List<CronParameters>>(taskHeader.CronParameters);
                var cronId = 0;
                foreach (var cron in crons!)
                {
                    var jobId = jobName + "_" + cronId;
                    var jobParam = new TaskJobParameters
                    {
                        TaskHeaderId = taskHeaderId,
                        Cts = CancellationToken.None,
                        GenerateFiles = true
                    };
                    if (taskHeader.Type == TaskType.Report && services!.ReportService)
                    {
                        var queueName = "report";
                        options.QueueName = queueName;
                        RecurringJob.AddOrUpdate(jobId, queueName, () => RunTaskJobAsync(jobParam, CancellationToken.None), cron.CronValue,
                            options);
                    }

                    if (taskHeader.Type == TaskType.Alert && services!.AlertService)
                    {
                        var queueName = "alert";
                        options.QueueName = queueName;
                        RecurringJob.AddOrUpdate(jobId, queueName, () => RunTaskJobAsync(jobParam, CancellationToken.None), cron.CronValue,
                            options);
                    }

                    if (taskHeader.Type == TaskType.DataTransfer && services!.DataTransferService)
                    {
                        var queueName = "datatransfer";
                        options.QueueName = queueName; //to remove in version 2.0
                        RecurringJob.AddOrUpdate(jobId, queueName, () => RunTaskJobAsync(jobParam, CancellationToken.None), cron.CronValue,
                            options);
                    }

                    cronId++;
                }
            }
        }
        else
        {
            List<RecurringJobDto> recurringJobs = JobStorage.Current.GetConnection().GetRecurringJobs();
            foreach (var j in recurringJobs.Where(a => a.Id.Contains(" Id:" + taskHeaderId + "_")))
                RecurringJob.RemoveIfExists(j.Id);
        }
    }

    public async ValueTask RunTaskJobAsync( TaskJobParameters parameters, CancellationToken cts )
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var db = scope.ServiceProvider.GetService<ApplicationDbContext>();
            parameters.Cts = cts;
            if (db != null)
            {
                using var handler = new BackgroundTaskHandler(db, _emailSender, _dbReader, _fileDeposit, _mapper,
                    _hostingEnvironment);
                await handler.HandleTask(parameters);

            }
        }

        //ReleaseMemory();
    }

    private void ReleaseMemory()
    {
        GC.Collect();
    }

    public async Task DeleteLocalFilesAsync()
    {
        ApplicationLogTask logTask = new()
            { StartDateTime = DateTime.Now, JobDescription = "File Cleaner", Type = "Cleaner service" };
        try
        {
            var qry = _context.ApplicationLogReportResult.Where(a => a.IsAvailable == true).GroupJoin(
                    _context.TaskHeader,
                    rr => rr.TaskHeaderId,
                    rh => rh.TaskHeaderId,
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
        ApplicationLogTask logTask = new()
            { StartDateTime = DateTime.Now, JobDescription = "Logs cleaner", Type = "Cleaner service" };
        var rententionDays =
            await _context.ApplicationParameters.Select(a => a.LogsRetentionInDays).FirstOrDefaultAsync();
        await _context.ApplicationLogSystem.Where(a => a.TimeStamp.Date < DateTime.Today.AddDays(-rententionDays))
            .ForEachAsync(a => _context.Remove(a));
        await _context.ApplicationLogTask.Where(a => a.EndDateTime.Date < DateTime.Today.AddDays(-rententionDays))
            .ForEachAsync(a => _context.Remove(a));
        await _context.ApplicationLogTaskDetails.Where(a => a.TimeStamp.Date < DateTime.Today.AddDays(-rententionDays))
            .ForEachAsync(a => _context.Remove(a));
        await _context.ApplicationLogEmailSender
            .Where(a => a.EndDateTime.Date < DateTime.Today.AddDays(-rententionDays))
            .ForEachAsync(a => _context.Remove(a));
        await _context.ApplicationAuditTrail.Where(a => a.DateTime.Date < DateTime.Today.AddDays(-rententionDays))
            .ForEachAsync(a => _context.Remove(a));
        await _context.ApplicationLogReportResult
            .Where(a => a.CreatedAt.Date < DateTime.Today.AddDays(-rententionDays) && a.IsAvailable == false)
            .ForEachAsync(a => _context.Remove(a));
        await _context.ApplicationLogQueryExecution
            .Where(a => a.StartDateTime.Date < DateTime.Today.AddDays(-rententionDays))
            .ForEachAsync(a => _context.Remove(a));
        await _context.ApplicationLogAdHocQueries
            .Where(a => a.StartDateTime.Date < DateTime.Today.AddDays(-rententionDays))
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