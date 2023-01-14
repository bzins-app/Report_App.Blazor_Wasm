using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.ApiExchanges;
using Report_App_WASM.Shared.Dashboard;

namespace Report_App_WASM.Server.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ILogger<DashboardController> logger,
        ApplicationDbContext context)
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
        AppMetrics metrics = new();
        var tasksToday = _context.ApplicationLogTask.Where(a => a.EndDateTime.Date == DateTime.Today.Date);
        metrics.NbrOfTasksExcecutedToday = await tasksToday.CountAsync();
        metrics.NbrTasksInError =
            await tasksToday.Where(a => a.Error == true && !a.Result!.Contains("attempt")).CountAsync();
        var reportsToday = _context.ApplicationLogReportResult.Where(a => a.IsAvailable == true);
        metrics.SizeFilesStoredLocally = await reportsToday.SumAsync(a => a.FileSizeInMb);
        metrics.NbrOfFilesStored = await reportsToday.CountAsync();

        var activeTask = _context.TaskHeader.Where(a => a.IsActivated);
        metrics.NbrOfActiveReports = await activeTask.Where(a => a.Type == TaskType.Report).CountAsync();
        metrics.NbrOfActiveAlerts = await activeTask.Where(a => a.Type == TaskType.Alert).CountAsync();
        metrics.NbrOfActiveDataTransfer = await activeTask.Where(a => a.Type == TaskType.DataTransfer).CountAsync();

        metrics.NbrOfActiveQueries = await _context.TaskDetail.Where(a => a.TaskHeader!.IsActivated).CountAsync();
        return metrics;
    }

    [HttpGet("TasksLogs")]
    public async Task<List<TaksLogsValues>> GetTasksLogsAsync()
    {
        return await _context.ApplicationLogTask.AsNoTracking()
            .Where(a => !string.IsNullOrEmpty(a.ActivityName) && a.EndDateTime.Date > DateTime.Today.AddDays(-20) &&
                        !a.Result.Contains("attempt"))
            .GroupBy(a => new { a.Type, a.ActivityName, a.EndDateTime.Date }).Select(a => new TaksLogsValues
            {
                Date = a.Key.Date, ActivityName = a.Key.ActivityName, TypeTask = a.Key.Type,
                TotalDuration = a.Sum(a => a.DurationInSeconds), NbrTasks = a.Count(),
                NbrErrors = a.Sum(a => a.Error ? 1 : 0)
            }).ToListAsync();
    }

    [HttpGet("SystemLogs")]
    public async Task<List<TaksSystemValues>> GetSystemLogsAsync()
    {
        return await _context.ApplicationLogSystem.AsNoTracking()
            .Where(a => a.Level > 2 && a.TimeStamp.Date > DateTime.Today.AddDays(-20))
            .GroupBy(a => a.TimeStamp.Date)
            .Select(a => new TaksSystemValues
            {
                Date = a.Key.Date,
                NbrWarnings = a.Sum(applicationLogSystem => applicationLogSystem.Level == 3 ? 1 : 0),
                NbrErrors = a.Sum(logSystem => logSystem.Level == 4 ? 1 : 0),
                NbrCriticals = a.Sum(a => a.Level == 5 ? 1 : 0)
            })
            .ToListAsync();
    }

    [HttpGet("EmailLogs")]
    public async Task<List<EmailsLogsValues>> GetEmailLogsAsync()
    {
        return await _context.ApplicationLogEmailSender.AsNoTracking()
            .Where(a => a.EndDateTime.Date > DateTime.Today.AddDays(-20))
            .GroupBy(a => a.EndDateTime.Date)
            .Select(a => new EmailsLogsValues
            {
                Date = a.Key.Date, NbrEmails = a.Count(), NbrErrors = a.Sum(a => a.Error ? 1 : 0),
                TotalDuration = a.Sum(a => a.DurationInSeconds)
            })
            .ToListAsync();
    }

    [HttpGet("StorageInfo")]
    public async Task<List<StorageData>> GetStorageInfoAsync()
    {
        return await _context.ApplicationLogReportResult
            .Where(a => a.CreatedAt > DateTime.Today.AddDays(-10) && a.IsAvailable && !a.Error)
            .GroupBy(a => a.ReportName)
            .Select(a => new StorageData { ReportName = a.Key, FileSizeInMb = a.Sum(b => b.FileSizeInMb) })
            .ToListAsync();
    }

    [HttpGet("DbFetchMetrics")]
    public async Task<List<DbLinesQuery>> GetDbFetchMetricsAsync()
    {
        return await _context.ApplicationLogQueryExecution.Where(a => a.EndDateTime > DateTime.Today.AddDays(-10))
            .GroupBy(a => new
            {
                Date = new DateTime(a.EndDateTime.Year, a.EndDateTime.Month, a.EndDateTime.Day, a.EndDateTime.Hour,
                    a.EndDateTime.Minute, 0)
            })
            .Select(a => new DbLinesQuery { Date = a.Key.Date, NbrOfRows = a.Sum(b => b.NbrOfRows) }).ToListAsync();
    }
}