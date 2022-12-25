using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Services.BackgroundWorker;
using Report_App_WASM.Server.Utils;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.ApiExchanges;

namespace Report_App_WASM.Server.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BackgroundWorkerController : ControllerBase
    {
        private readonly ILogger<DataGridController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IBackgroundWorkers _backgroundWorkers;

        public BackgroundWorkerController(ILogger<DataGridController> logger,
            ApplicationDbContext context, IMapper mapper, IBackgroundWorkers backgroundWorkers)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _backgroundWorkers = backgroundWorkers;
        }

        private async Task<SubmitResult> ActivateBackgroundWorkersAsync(bool activate, BackgroundTaskType type)
        {
            var result = await _backgroundWorkers.ActivateBackgroundWorkersAsync(activate, type);

            return result;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateServicesStatusAsync(ApiCrudPayload<ServicesStatus> status)
        {
            try
            {
#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.ServicesStatus?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Entry<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.ServicesStatus?' doesn't match 'class' constraint.
                _context.Entry(status.EntityValue).State = EntityState.Modified;
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.ServicesStatus?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Entry<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.ServicesStatus?' doesn't match 'class' constraint.
                await SaveDbAsync(status.UserName);
#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.ServicesStatus?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Entry<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.ServicesStatus?' doesn't match 'class' constraint.
                _context.Entry(status.EntityValue).State = EntityState.Detached;
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.ServicesStatus?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Entry<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.ServicesStatus?' doesn't match 'class' constraint.
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        private async Task<SubmitResult> UpdateServicesAsync(ServicesStatus item, string? userName)
        {
            try
            {
                _context.Entry(item).State = EntityState.Modified;
                await SaveDbAsync(userName);
                _context.Entry(item).State = EntityState.Detached;
                return new() { Success = true };
            }
            catch (Exception ex)
            {
                return new() { Success = false, Message = ex.Message };
            }
        }

        private async Task<ServicesStatus> GetServiceStatusAsync()
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await _context.ServicesStatus.OrderBy(a => a.Id).FirstOrDefaultAsync();
#pragma warning restore CS8603 // Possible null reference return.
        }

        [HttpPost]
        public async Task<IActionResult> ActivateReportService(ApiCrudPayload<ApiBackgrounWorkerdPayload> value)
        {
            var item = await GetServiceStatusAsync();
            item.ReportService = value.EntityValue!.Activate;
            var result = await ActivateBackgroundWorkersAsync(value.EntityValue.Activate, BackgroundTaskType.Report);
            await UpdateServicesAsync(item, value.UserName);
            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> ActivateAlertService(ApiCrudPayload<ApiBackgrounWorkerdPayload> value)
        {
            var item = await GetServiceStatusAsync();
            item.AlertService = value.EntityValue!.Activate;
            var result = await ActivateBackgroundWorkersAsync(value.EntityValue.Activate, BackgroundTaskType.Alert);
            await UpdateServicesAsync(item, value.UserName);
            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> ActivateDataTransferService(ApiCrudPayload<ApiBackgrounWorkerdPayload> value)
        {
            var item = await GetServiceStatusAsync();
            item.DataTransferService = value.EntityValue!.Activate;
            var result = await ActivateBackgroundWorkersAsync(value.EntityValue.Activate, BackgroundTaskType.DataTransfer);
            await UpdateServicesAsync(item, value.UserName);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> ActivateCleanerService(ApiCrudPayload<ApiBackgrounWorkerdPayload> value)
        {
            var item = await GetServiceStatusAsync();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            item.CleanerService = value.EntityValue.Activate;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            var result = await ActivateBackgroundWorkersAsync(value.EntityValue.Activate, BackgroundTaskType.Cleaner);
            await UpdateServicesAsync(item, value.UserName);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> ActivatePerActivity(ApiCrudPayload<ApiBackgrounWorkerdPayload> value)
        {
            await _backgroundWorkers.SwitchBackgroundTasksPerActivityAsync(value.EntityValue!.Value, value.EntityValue.Activate);
            return Ok(new SubmitResult { Success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ActivatePerTask(ApiCrudPayload<ApiBackgrounWorkerdPayload> value)
        {
            await _backgroundWorkers.SwitchBackgroundTaskAsync(value.EntityValue!.Value, value.EntityValue.Activate);
            return Ok(new SubmitResult { Success = true });
        }

        [HttpPost]
        public IActionResult RunManually(ApiCrudPayload<RunTaskManually> value)
        {
#pragma warning disable CS8604 // Possible null reference argument for parameter 'customQueryParameters' in 'void IBackgroundWorkers.RunManuallyTask(int taskHeaderId, string? runBy, List<EmailRecipient> emails, List<QueryCommandParameter> customQueryParameters, bool generateFiles = false)'.
#pragma warning disable CS8604 // Possible null reference argument for parameter 'emails' in 'void IBackgroundWorkers.RunManuallyTask(int taskHeaderId, string? runBy, List<EmailRecipient> emails, List<QueryCommandParameter> customQueryParameters, bool generateFiles = false)'.
            _backgroundWorkers.RunManuallyTask(value.EntityValue!.TaskHeaderId, value.UserName, value.EntityValue.Emails, value.EntityValue.CustomQueryParameters, value.EntityValue.GenerateFiles);
#pragma warning restore CS8604 // Possible null reference argument for parameter 'emails' in 'void IBackgroundWorkers.RunManuallyTask(int taskHeaderId, string? runBy, List<EmailRecipient> emails, List<QueryCommandParameter> customQueryParameters, bool generateFiles = false)'.
#pragma warning restore CS8604 // Possible null reference argument for parameter 'customQueryParameters' in 'void IBackgroundWorkers.RunManuallyTask(int taskHeaderId, string? runBy, List<EmailRecipient> emails, List<QueryCommandParameter> customQueryParameters, bool generateFiles = false)'.
            return Ok(new SubmitResult { Success = true });
        }

        private async Task SaveDbAsync(string? userId = "system")
        {
            await _context.SaveChangesAsync(userId);
        }
    }
}
