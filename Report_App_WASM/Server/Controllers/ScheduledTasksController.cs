using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.ExternalApi;

namespace Report_App_WASM.Server.Controllers;

[Authorize]
[Route("ExternalApi/[controller]/[Action]")]
[ApiController]
public class ScheduledTasksController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ScheduledTasksController> _logger;

    public ScheduledTasksController(ILogger<ScheduledTasksController> logger,
        ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
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
}