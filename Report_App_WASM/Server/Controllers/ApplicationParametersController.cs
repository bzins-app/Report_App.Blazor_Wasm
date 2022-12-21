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
    public class ApplicationParametersController : ControllerBase
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

        [Authorize]
        [HttpGet("ActivitiesInfo")]
        public async Task<IEnumerable<SelectItemActivitiesInfo>> GetActivitiesInfo()
        {
            return await _context.Activity.AsNoTracking().Select(a => new SelectItemActivitiesInfo { ActivityId = a.ActivityId, ActivityName = a.ActivityName, HasALogo = !string.IsNullOrEmpty(a.ActivityLogo), IsVisible = a.Display, LogoPath = a.ActivityLogo, IsActivated = a.IsActivated }).ToArrayAsync();
        }

        [HttpGet("ApplicationConstants")]
        public ApplicationConstantsValues GetApplicationConstants()
        {
            ApplicationConstantsValues values = new() { ApplicationLogo = ApplicationConstants.ApplicationLogo, ApplicationName = ApplicationConstants.ApplicationName, LDAPLogin = ApplicationConstants.LDAPLogin };
            return values;
        }

        [Authorize]
        [HttpGet("CheckSMTPConfiguration")]
        public async Task<bool> CheckSMTPConfigurationAsync()
        {
            return await _context.SMTPConfiguration.Where(a => a.IsActivated).AnyAsync();
        }

        [Authorize]
        [HttpGet("GetUploadedFilePath")]
        public Tuple<string, string> GetUploadedFilePath(string fileName)
        {
            var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "upload");
            var filePath = Path.Combine(uploads, fileName);
            string savePath = "upload/" + fileName;
            Tuple<string, string> result = new(savePath, filePath);
            return result;
        }

        [Authorize]
        [HttpGet("GetApplicationParameters")]
        public async Task<ApplicationParameters> GetApplicationParametersAsync()
        {
            return await _context.ApplicationParameters.OrderBy(a => a.Id).AsNoTrackingWithIdentityResolution().FirstOrDefaultAsync();
        }
    }
}
