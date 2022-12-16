using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Utils;
using Report_App_WASM.Shared;

namespace Report_App_WASM.Server.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class ApplicationParametersController : ControllerBase
    {
        private readonly ILogger<ApplicationParametersController> _logger;
        private readonly ApplicationDbContext _context;

        public ApplicationParametersController(ILogger<ApplicationParametersController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("ActivitiesInfo")]
        public async Task<IEnumerable<SelectItemActivitiesInfo>> GetActivitiesInfo()
        {
            return await _context.Activity.AsNoTracking().Select(a => new SelectItemActivitiesInfo { ActivityId = a.ActivityId, ActivityName = a.ActivityName, HasALogo = !string.IsNullOrEmpty(a.ActivityLogo), IsVisible = a.Display, LogoPath = a.ActivityLogo }).ToArrayAsync();
        }

        [HttpGet("ApplicationConstants")]
        public ApplicationConstantsValues GetApplicationConstants()
        {
            ApplicationConstantsValues values = new() { ApplicationLogo = ApplicationConstants.ApplicationLogo, ApplicationName = ApplicationConstants.ApplicationName };
            return values;
        }
    }
}
