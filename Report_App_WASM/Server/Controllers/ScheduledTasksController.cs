using System.Text.Json;
using Report_App_WASM.Shared.ExternalApi;

namespace Report_App_WASM.Server.Controllers;

[Authorize]
[Route("ExternalApi/[controller]/[Action]")]
[ApiController]
public class ScheduledTasksController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ScheduledTasksController> _logger;
    private readonly IBackgroundWorkers _backgroundWorkers;

    public ScheduledTasksController(ILogger<ScheduledTasksController> logger,
        ApplicationDbContext context, IBackgroundWorkers backgroundWorkers)
    {
        _logger = logger;
        _context = context;
        _backgroundWorkers = backgroundWorkers;
    }

    [HttpGet]
    public async Task<List<TasksInfo>> GetAlertTasksListAsync(string? activityName)
    {
        return await GetTasksListAsync(activityName, TaskType.Alert);
    }

    [HttpGet]
    public async Task<List<TasksInfo>> GetDataTransferTasksListAsync(string? activityName)
    {
        return await GetTasksListAsync(activityName, TaskType.DataTransfer);
    }

    [HttpGet]
    public async Task<List<TasksInfo>> GetReportsTasksListAsync(string? activityName)
    {
        return await GetTasksListAsync(activityName, TaskType.Report, true);
    }

    private async Task<List<TasksInfo>> GetTasksListAsync(string? activityName, TaskType taskType,
        bool includeFileDetails = false)
    {
        var query = _context.ScheduledTask.Where(a => a.Type == taskType);

        if (!string.IsNullOrEmpty(activityName))
        {
            query = query.Where(a => a.DataProvider.ProviderName == activityName);
        }

        return await query.Select(a => new TasksInfo
        {
            TaksId = a.ScheduledTaskId,
            ActivityName = a.DataProvider.ProviderName,
            LastRunDateTime = a.LastRunDateTime,
            IsActivated = a.IsEnabled,
            TaskName = a.TaskName,
            TaskType = a.Type.ToString(),
            QueriesName = a.TaskQueries.Select(td => td.QueryName).ToList(),
            HasADepositPath = includeFileDetails && a.FileStorageLocationId != 0,
            TypeOfGeneratedFile = includeFileDetails ? a.TypeFileName : null
        }).ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = "ApiAccess")]
    public async Task<IActionResult> EnqueueTask(ApiRunTask payload)
    {
        if (payload == null) return BadRequest("Payload null");

        var taskHeader = await _context.ScheduledTask.Include(a => a.DistributionLists)
            .FirstOrDefaultAsync(a => a.ScheduledTaskId == payload.ScheduledTaskId);

        if (taskHeader == null) return BadRequest("Task id not found");

        var recipients = new List<EmailRecipient>();
        if (payload.SendEmail)
        {
            var emailSerialized = taskHeader.DistributionLists.FirstOrDefault()?.Recipients;
            if (!string.IsNullOrEmpty(emailSerialized))
            {
                recipients = JsonSerializer.Deserialize<List<EmailRecipient>>(emailSerialized) ??
                             new List<EmailRecipient>();
            }
        }

        var parameters = !string.IsNullOrEmpty(taskHeader.GlobalQueryParameters)
            ? JsonSerializer.Deserialize<List<QueryCommandParameter>>(taskHeader.GlobalQueryParameters) ??
              new List<QueryCommandParameter>()
            : new List<QueryCommandParameter>();

        await _backgroundWorkers.RunManuallyTask(payload.ScheduledTaskId, User?.Identity?.Name, recipients, parameters,
            payload.GenerateFileToFolder);

        return Ok("Task enqueued successfully");
    }
}