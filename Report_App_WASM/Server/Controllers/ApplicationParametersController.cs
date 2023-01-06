using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Utils;
using Report_App_WASM.Shared;

namespace Report_App_WASM.Server.Controllers
{

    [ApiExplorerSettings(IgnoreApi = true)]
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationParametersController : ControllerBase, IDisposable
    {
        private readonly ILogger<ApplicationParametersController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ApplicationParametersController(ILogger<ApplicationParametersController> logger,
            ApplicationDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        [Authorize]
        [HttpGet("ActivitiesInfo")]
        public async Task<IEnumerable<SelectItemActivitiesInfo>> GetActivitiesInfo()
        {
            return await _context.Activity.AsNoTracking().Select(a => new SelectItemActivitiesInfo { ActivityId = a.ActivityId, ActivityName = a.ActivityName, HasALogo = !string.IsNullOrEmpty(a.ActivityLogo), IsVisible = a.Display, LogoPath = a.ActivityLogo, IsActivated = a.IsActivated }).ToArrayAsync();
        }

        [Authorize]
        [HttpGet("SftpInfo")]
        public async Task<IEnumerable<SelectItem>> GetSftpInfo()
        {
            return await _context.SftpConfiguration.Select(a => new SelectItem { Id = a.SftpConfigurationId, Name = a.ConfigurationName }).ToListAsync();
        }

        [Authorize]
        [HttpGet("DepositPathInfo")]
        public async Task<IEnumerable<SelectItem>> GetDepositPathInfo()
        {
            return await _context.FileDepositPathConfiguration.Select(a => new SelectItem { Id = a.FileDepositPathConfigurationId, Name = a.ConfigurationName }).ToListAsync();
        }

        [HttpGet("ApplicationConstants")]
        public ApplicationConstantsValues GetApplicationConstants()
        {
            ApplicationConstantsValues values = new()
            {
                ApplicationLogo = ApplicationConstants.ApplicationLogo,
                ApplicationName = ApplicationConstants.ApplicationName,
                LdapLogin = ApplicationConstants.LdapLogin,
                WindowsEnv = ApplicationConstants.WindowsEnv,
                ActivateAdHocQueriesModule = ApplicationConstants.ActivateAdHocQueriesModule,
                ActivateTaskSchedulerModule = ApplicationConstants.ActivateTaskSchedulerModule
            };
            return values;
        }

        [Authorize]
        [HttpGet("CheckSmtpConfiguration")]
        public async Task<bool> CheckSmtpConfigurationAsync()
        {
            return await _context.SmtpConfiguration.Where(a => a.IsActivated).AnyAsync();
        }

        [Authorize]
        [HttpGet("CheckTaskHeaderEmail")]
        public async Task<bool> CheckTaskHeaderEmailAsync(int taskHeaderId)
        {
            return await _context.TaskEmailRecipient.Where(a => a.TaskHeader!.TaskHeaderId == taskHeaderId).Select(a => a.Email).FirstOrDefaultAsync() != "[]" || await _context.TaskEmailRecipient.Where(a => a.TaskHeader.TaskHeaderId == taskHeaderId).AnyAsync();
        }


        [Authorize]
        [HttpGet("GetUploadedFilePath")]
        public Tuple<string, string> GetUploadedFilePath(string fileName)
        {
            var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "upload");
            var filePath = Path.Combine(uploads, fileName);
            var savePath = "upload/" + fileName;
            Tuple<string, string> result = new(savePath, filePath);
            return result;
        }

        [Authorize]
        [HttpGet("GetApplicationParameters")]
        public async Task<ApplicationParameters?> GetApplicationParametersAsync()
        {
            return await _context.ApplicationParameters.OrderBy(a => a.Id).FirstOrDefaultAsync();
        }
    }
}
