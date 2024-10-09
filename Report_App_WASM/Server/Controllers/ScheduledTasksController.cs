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
        var query = _context.TaskHeader.Where(a => a.Type == taskType);

        if (!string.IsNullOrEmpty(activityName))
        {
            query = query.Where(a => a.Activity.ActivityName == activityName);
        }

        return await query.Select(a => new TasksInfo
        {
            TaksId = a.TaskHeaderId,
            ActivityName = a.Activity.ActivityName,
            LastRunDateTime = a.LastRunDateTime,
            IsActivated = a.IsActivated,
            TaskName = a.TaskName,
            TaskType = a.Type.ToString(),
            QueriesName = a.TaskDetails.Select(td => td.QueryName).ToList(),
            HasADepositPath = includeFileDetails && a.FileDepositPathConfigurationId != 0,
            TypeOfGeneratedFile = includeFileDetails ? a.TypeFileName : null
        }).ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = "ApiAccess")]
    public async Task<IActionResult> EnqueueTask(ApiRunTask payload)
    {
        if (payload == null) return BadRequest("Payload null");

        var taskHeader = await _context.TaskHeader.Include(a => a.TaskEmailRecipients)
            .FirstOrDefaultAsync(a => a.TaskHeaderId == payload.TaksId);

        if (taskHeader == null) return BadRequest("Task id not found");

        var recipients = new List<EmailRecipient>();
        if (payload.SendEmail)
        {
            var emailSerialized = taskHeader.TaskEmailRecipients.FirstOrDefault()?.Email;
            if (!string.IsNullOrEmpty(emailSerialized))
            {
                recipients = JsonSerializer.Deserialize<List<EmailRecipient>>(emailSerialized) ??
                             new List<EmailRecipient>();
            }
        }

        var parameters = !string.IsNullOrEmpty(taskHeader.QueryParameters)
            ? JsonSerializer.Deserialize<List<QueryCommandParameter>>(taskHeader.QueryParameters) ??
              new List<QueryCommandParameter>()
            : new List<QueryCommandParameter>();

        _backgroundWorkers.RunManuallyTask(payload.TaksId, User?.Identity?.Name, recipients, parameters,
            payload.GenerateFileToFolder);

        return Ok("Task enqueued successfully");
    }
}