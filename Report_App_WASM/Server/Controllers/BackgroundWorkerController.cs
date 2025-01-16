using Report_App_WASM.Server.Utils.BackgroundWorker;

namespace Report_App_WASM.Server.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
[Route("api/[controller]/[action]")]
[ApiController]
public class BackgroundWorkerController : ControllerBase, IDisposable
{
    private readonly IBackgroundWorkers _backgroundWorkers;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataGridController> _logger;
    private readonly IMapper _mapper;

    public BackgroundWorkerController(ILogger<DataGridController> logger,
        ApplicationDbContext context, IMapper mapper, IBackgroundWorkers backgroundWorkers)
    {
        _logger = logger;
        _context = context;
        _mapper = mapper;
        _backgroundWorkers = backgroundWorkers;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private async Task<SubmitResult> ActivateBackgroundWorkersAsync(bool activate, BackgroundTaskType type)
    {
        return await _backgroundWorkers.ActivateBackgroundWorkersAsync(activate, type);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateServicesStatusAsync(ApiCrudPayload<SystemServicesStatus> status)
    {
        if (status.EntityValue == null)
        {
            return Ok(new SubmitResult { Success = false, Message = "EntityValue cannot be null." });
        }

        try
        {
            _context.Entry(status.EntityValue).State = EntityState.Modified;
            await SaveDbAsync(status.UserName);
            _context.Entry(status.EntityValue).State = EntityState.Detached;
            return Ok(new SubmitResult { Success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service status");
            return Ok(new SubmitResult { Success = false, Message = ex.Message });
        }
    }

    private async Task<SubmitResult> UpdateServicesAsync(SystemServicesStatus item, string? userName)
    {
        try
        {
            _context.Entry(item).State = EntityState.Modified;
            await SaveDbAsync(userName);
            _context.Entry(item).State = EntityState.Detached;
            return new SubmitResult { Success = true };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating services");
            return new SubmitResult { Success = false, Message = ex.Message };
        }
    }

    private async Task<SystemServicesStatus> GetServiceStatusAsync()
    {
        return await _context.SystemServicesStatus.OrderBy(a => a.Id).FirstOrDefaultAsync() ??
               new SystemServicesStatus();
    }

    private async Task<IActionResult> ActivateServiceAsync(ApiCrudPayload<ApiBackgroundWorkerPayload> value,
        BackgroundTaskType type, Action<SystemServicesStatus, bool> changeStatus)
    {
        var item = await GetServiceStatusAsync();
        changeStatus(item, value.EntityValue!.Activate);
        var result = await ActivateBackgroundWorkersAsync(value.EntityValue.Activate, type);
        await UpdateServicesAsync(item, value.UserName);
        return Ok(result);
    }

    [HttpPost]
    public Task<IActionResult> ActivateReportService(ApiCrudPayload<ApiBackgroundWorkerPayload> value)
    {
        return ActivateServiceAsync(value, BackgroundTaskType.Report,
            (item, activate) => item.ReportService = activate);
    }

    [HttpPost]
    public Task<IActionResult> ActivateAlertService(ApiCrudPayload<ApiBackgroundWorkerPayload> value)
    {
        return ActivateServiceAsync(value, BackgroundTaskType.Alert, (item, activate) => item.AlertService = activate);
    }

    [HttpPost]
    public Task<IActionResult> ActivateDataTransferService(ApiCrudPayload<ApiBackgroundWorkerPayload> value)
    {
        return ActivateServiceAsync(value, BackgroundTaskType.DataTransfer,
            (item, activate) => item.DataTransferService = activate);
    }

    [HttpPost]
    public Task<IActionResult> ActivateCleanerService(ApiCrudPayload<ApiBackgroundWorkerPayload> value)
    {
        return ActivateServiceAsync(value, BackgroundTaskType.Cleaner,
            (item, activate) => item.CleanerService = activate);
    }

    [HttpPost]
    public async Task<IActionResult> ActivatePerActivity(ApiCrudPayload<ApiBackgroundWorkerPayload> value)
    {
        await _backgroundWorkers.SwitchBackgroundTasksPerActivityAsync(value.EntityValue!.Value,
            value.EntityValue.Activate);
        return Ok(new SubmitResult { Success = true });
    }

    [HttpPost]
    public async Task<IActionResult> ActivatePerTask(ApiCrudPayload<ApiBackgroundWorkerPayload> value)
    {
        await _backgroundWorkers.SwitchBackgroundTaskAsync(value.EntityValue!.Value, value.EntityValue.Activate);
        return Ok(new SubmitResult { Success = true });
    }

    [HttpPost]
    public async Task<IActionResult> RunManually(ApiCrudPayload<RunTaskManually> value)
    {
        await _backgroundWorkers.RunManuallyTask(value.EntityValue!.TaskHeaderId, value.UserName, value.EntityValue.Emails!,
            value.EntityValue.QueryCommandParameters!, value.EntityValue.GenerateFiles);
        return Ok(new SubmitResult { Success = true });
    }

    private async Task SaveDbAsync(string? userId = "system")
    {
        await _context.SaveChangesAsync(userId);
    }
}