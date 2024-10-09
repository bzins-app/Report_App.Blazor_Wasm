using Report_App_WASM.Shared.Dashboard;

namespace Report_App_WASM.Server.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase, IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ILogger<DashboardController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    [HttpGet("Metrics")]
    public async Task<AppMetrics> GetAppMetricsAsync()
    {
        var today = DateTime.Today.Date;
        var tasksToday = _context.ApplicationLogTask.Where(a => a.EndDateTime.Date == today);
        var reportsToday = _context.ApplicationLogReportResult.Where(a => a.IsAvailable);

        var servicesStatus = await _context.ServicesStatus.FirstOrDefaultAsync();
        var activeTask = _context.TaskHeader.Where(a => a.IsActivated && a.Activity.IsActivated);

        var tasksTodayList = await tasksToday.ToListAsync();
        var reportsTodayList = await reportsToday.ToListAsync();
        var activeTaskList = await activeTask.ToListAsync();

        var metrics = new AppMetrics
        {
            NbrOfTasksExcecutedToday = tasksTodayList.Count,
            NbrTasksInError = tasksTodayList.Count(a => a.Error && !a.Result!.Contains("attempt")),
            SizeFilesStoredLocally = reportsTodayList.Sum(a => a.FileSizeInMb),
            NbrOfFilesStored = reportsTodayList.Count,
            NbrOfActiveReports = servicesStatus.ReportService
                ? activeTaskList.Count(a => a.Type == TaskType.Report)
                : 0,
            NbrOfActiveAlerts = servicesStatus.AlertService
                ? activeTaskList.Count(a => a.Type == TaskType.Alert)
                : 0,
            NbrOfActiveDataTransfer = servicesStatus.DataTransferService
                ? activeTaskList.Count(a => a.Type == TaskType.DataTransfer)
                : 0,
            NbrOfActiveQueries = await _context.TaskDetail.CountAsync(a =>
                a.TaskHeader!.IsActivated && a.TaskHeader.Activity.IsActivated &&
                ((a.TaskHeader.Type == TaskType.Report && servicesStatus.ReportService) ||
                 (a.TaskHeader.Type == TaskType.Alert && servicesStatus.AlertService) ||
                 (a.TaskHeader.Type == TaskType.DataTransfer && servicesStatus.DataTransferService)))
        };

        return metrics;
    }

    [HttpGet("TasksLogs")]
    public async Task<List<TaksLogsValues>> GetTasksLogsAsync()
    {
        var dateThreshold = DateTime.Today.AddDays(-20);
        return await _context.ApplicationLogTask.AsNoTracking()
            .Where(a => !string.IsNullOrEmpty(a.ActivityName) && a.EndDateTime.Date > dateThreshold &&
                        !a.Result.Contains("attempt"))
            .GroupBy(a => new { a.Type, a.ActivityName, a.EndDateTime.Date })
            .Select(a => new TaksLogsValues
            {
                Date = a.Key.Date,
                ActivityName = a.Key.ActivityName,
                TypeTask = a.Key.Type,
                TotalDuration = a.Sum(a => a.DurationInSeconds),
                NbrTasks = a.Count(),
                NbrErrors = a.Sum(a => a.Error ? 1 : 0)
            }).ToListAsync();
    }

    [HttpGet("SystemLogs")]
    public async Task<List<TaksSystemValues>> GetSystemLogsAsync()
    {
        var dateThreshold = DateTime.Today.AddDays(-20);
        return await _context.ApplicationLogSystem.AsNoTracking()
            .Where(a => a.Level > 2 && a.TimeStamp.Date > dateThreshold)
            .GroupBy(a => a.TimeStamp.Date)
            .Select(a => new TaksSystemValues
            {
                Date = a.Key.Date,
                NbrWarnings = a.Sum(log => log.Level == 3 ? 1 : 0),
                NbrErrors = a.Sum(log => log.Level == 4 ? 1 : 0),
                NbrCriticals = a.Sum(log => log.Level == 5 ? 1 : 0)
            }).ToListAsync();
    }

    [HttpGet("EmailLogs")]
    public async Task<List<EmailsLogsValues>> GetEmailLogsAsync()
    {
        var dateThreshold = DateTime.Today.AddDays(-20);
        return await _context.ApplicationLogEmailSender.AsNoTracking()
            .Where(a => a.EndDateTime.Date > dateThreshold)
            .GroupBy(a => a.EndDateTime.Date)
            .Select(a => new EmailsLogsValues
            {
                Date = a.Key.Date,
                NbrEmails = a.Count(),
                NbrErrors = a.Sum(a => a.Error ? 1 : 0),
                TotalDuration = a.Sum(a => a.DurationInSeconds)
            }).ToListAsync();
    }

    [HttpGet("StorageInfo")]
    public async Task<List<StorageData>> GetStorageInfoAsync()
    {
        var dateThreshold = DateTime.Today.AddDays(-10);
        return await _context.ApplicationLogReportResult
            .Where(a => a.CreatedAt > dateThreshold && a.IsAvailable && !a.Error)
            .GroupBy(a => a.ReportName)
            .Select(a => new StorageData { ReportName = a.Key, FileSizeInMb = a.Sum(b => b.FileSizeInMb) })
            .ToListAsync();
    }

    [HttpGet("DbFetchMetrics")]
    public async Task<List<DbLinesQuery>> GetDbFetchMetricsAsync()
    {
        var dateThreshold = DateTime.Today.AddDays(-10);
        return await _context.ApplicationLogQueryExecution
            .Where(a => a.EndDateTime > dateThreshold)
            .GroupBy(a => new DateTime(a.EndDateTime.Year, a.EndDateTime.Month, a.EndDateTime.Day, a.EndDateTime.Hour,
                a.EndDateTime.Minute, 0))
            .Select(a => new DbLinesQuery { Date = a.Key, NbrOfRows = a.Sum(b => b.NbrOfRows) })
            .ToListAsync();
    }
}
