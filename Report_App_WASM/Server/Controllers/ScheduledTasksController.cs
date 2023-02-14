using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Services.BackgroundWorker;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.ExternalApi;
using Report_App_WASM.Shared.SerializedParameters;

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
        var info = new List<TasksInfo>();
        var query = _context.TaskHeader.Where(a => a.Type == TaskType.Alert).AsQueryable();
        if (!string.IsNullOrEmpty(activityName))
            query = query.Where(a => a.Activity.ActivityName == activityName).AsQueryable();

        var feedback = await query.Select(a => new TasksInfo
        {
            TaksId = a.TaskHeaderId, ActivityName = a.Activity.ActivityName, LastRunDateTime = a.LastRunDateTime,
            IsActivated = a.IsActivated, TaskName = a.TaskName, TaskType = a.Type.ToString(),
            QueriesName = a.TaskDetails.Select(a => a.QueryName).ToList()
        }).ToListAsync();
        if (feedback != null) info = feedback;

        return info;
    }

    [HttpGet]
    public async Task<List<TasksInfo>> GetDataTransferTasksListAsync(string? activityName)
    {
        var info = new List<TasksInfo>();
        var query = _context.TaskHeader.Where(a => a.Type == TaskType.DataTransfer).AsQueryable();
        if (!string.IsNullOrEmpty(activityName))
            query = query.Where(a => a.Activity.ActivityName == activityName).AsQueryable();

        var feedback = await query.Select(a => new TasksInfo
        {
            TaksId = a.TaskHeaderId,
            ActivityName = a.Activity.ActivityName,
            LastRunDateTime = a.LastRunDateTime,
            IsActivated = a.IsActivated,
            TaskName = a.TaskName,
            TaskType = a.Type.ToString(),
            QueriesName = a.TaskDetails.Select(a => a.QueryName).ToList()
        }).ToListAsync();
        if (feedback != null) info = feedback;

        return info;
    }

    [HttpGet]
    public async Task<List<TasksInfo>> GetReportsTasksListAsync(string? activityName)
    {
        var info = new List<TasksInfo>();
        var query = _context.TaskHeader.Where(a => a.Type == TaskType.Report).AsQueryable();
        if (!string.IsNullOrEmpty(activityName))
            query = query.Where(a => a.Activity.ActivityName == activityName).AsQueryable();

        var feedback = await query.Select(a => new TasksInfo
        {
            TaksId = a.TaskHeaderId,
            ActivityName = a.Activity.ActivityName,
            LastRunDateTime = a.LastRunDateTime,
            HasADepositPath = a.FileDepositPathConfigurationId != 0,
            IsActivated = a.IsActivated,
            TaskName = a.TaskName,
            TaskType = a.Type.ToString(),
            QueriesName = a.TaskDetails.Select(a => a.QueryName).ToList(), TypeOfGeneratedFile = a.TypeFileName
        }).ToListAsync();
        if (feedback != null) info = feedback;

        return info;
    }

    [HttpPost]
    [Authorize(Roles = "ApiAccess")]
    public async Task<IActionResult> EnqueueTask(ApiRunTask payload)
    {
        if (payload == null) return BadRequest("Payload null");
        List<EmailRecipient> recipients = new();
        List<QueryCommandParameter> parameters = new();
        var _th = await _context.TaskHeader.Include(a => a.TaskEmailRecipients)
            .FirstOrDefaultAsync(a => a.TaskHeaderId == payload.TaksId);
        if (_th == null) return BadRequest("Task id not found");
        if (payload.SendEmail)
        {
            var _mailSerialized = _th.TaskEmailRecipients.FirstOrDefault().Email;
            if (!string.IsNullOrEmpty(_mailSerialized))
            {
                recipients = JsonSerializer.Deserialize<List<EmailRecipient>>(_mailSerialized)!;
                if (recipients == null) recipients = new List<EmailRecipient>();
            }
        }

        if (!string.IsNullOrEmpty(_th.QueryParameters))
            parameters = JsonSerializer.Deserialize<List<QueryCommandParameter>>(_th.QueryParameters)! ??
                         new List<QueryCommandParameter>();


        _backgroundWorkers.RunManuallyTask(payload.TaksId, User?.Identity?.Name, recipients, parameters,
            payload.GenerateFileToFolder);

        return Ok("Task enqueued successfully");
    }
}