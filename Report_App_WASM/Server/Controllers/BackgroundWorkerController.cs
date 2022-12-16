using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Shared;
using ReportAppWASM.Server.Services.BackgroundWorker;

namespace Report_App_WASM.Server.Controllers
{
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

        private async Task<SubmitResult> ActivateBackgroundWorkersAsync(bool activate, string type)
        {
            return await _backgroundWorkers.ActivateBackgroundWorkersAsync(activate, type);
        }

        [HttpPost]
        public async Task UpdateServicesStatusAsync(ApiCRUDPayload<ServicesStatus> status)
        {
            _context.Entry(status.EntityValue).State = EntityState.Modified;
            await SaveDbAsync(status.UserName);
            _context.Entry(status.EntityValue).State = EntityState.Detached;
        }

        [HttpPost]
        public async Task<IActionResult> ActivateReportService(bool value)
        {
            var result = await ActivateBackgroundWorkersAsync(value, "Report");
            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> ActivateAlertService(bool value)
        {
            var result = await ActivateBackgroundWorkersAsync(value, "Alert");
            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> ActivateDataTransferService(bool value)
        {
            var result = await ActivateBackgroundWorkersAsync(value, "DataTransfer");
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ActivateCleanerService(bool value)
        {
            var result = await ActivateBackgroundWorkersAsync(value, "cleaner");
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ActivatePerActivity(ApiBackgrounWorkerdPayload value)
        {
            await _backgroundWorkers.SwitchBackgroundTasksPerActivityAsync(value.Value, value.Activate);
            return Ok();
        }

        private async Task SaveDbAsync(string userId = "system")
        {
            await _context.SaveChangesAsync(userId);
        }
    }
}
