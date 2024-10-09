namespace Report_App_WASM.Server.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[ApiController]
[Route("api/[controller]")]
public class ApplicationParametersController : ControllerBase, IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly ILogger<ApplicationParametersController> _logger;

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
        return await _context.Activity
            .Where(a => a.ActivityType == ActivityType.SourceDb)
            .AsNoTracking()
            .Select(a => new SelectItemActivitiesInfo
            {
                ActivityId = a.ActivityId,
                ActivityName = a.ActivityName,
                HasALogo = !string.IsNullOrEmpty(a.ActivityLogo),
                IsVisible = a.Display,
                LogoPath = a.ActivityLogo,
                IsActivated = a.IsActivated,
                DbConnectionId = a.ActivityDbConnections.FirstOrDefault().Id
            })
            .ToArrayAsync();
    }

    [Authorize]
    [HttpGet("DataTransfers")]
    public async Task<IEnumerable<SelectItemActivitiesInfo>> GetDataTransfersInfo()
    {
        return await _context.Activity
            .Where(a => a.ActivityType == ActivityType.TargetDb)
            .AsNoTracking()
            .Select(a => new SelectItemActivitiesInfo
            {
                ActivityId = a.ActivityId,
                ActivityName = a.ActivityName,
                HasALogo = !string.IsNullOrEmpty(a.ActivityLogo),
                IsVisible = a.Display,
                LogoPath = a.ActivityLogo,
                IsActivated = a.IsActivated,
                DbConnectionId = a.ActivityDbConnections.FirstOrDefault().Id
            })
            .ToArrayAsync();
    }

    [Authorize]
    [HttpGet("SftpInfo")]
    public async Task<IEnumerable<SelectItem>> GetSftpInfo()
    {
        return await _context.SftpConfiguration
            .Select(a => new SelectItem { Id = a.SftpConfigurationId, Name = a.ConfigurationName })
            .ToListAsync();
    }

    [Authorize]
    [HttpGet("DepositPathInfo")]
    public async Task<IEnumerable<SelectItem>> GetDepositPathInfo()
    {
        return await _context.FileDepositPathConfiguration
            .Select(a => new SelectItem { Id = a.FileDepositPathConfigurationId, Name = a.ConfigurationName })
            .ToListAsync();
    }

    [HttpGet("ApplicationConstants")]
    public ApplicationConstantsValues GetApplicationConstants()
    {
        return new ApplicationConstantsValues
        {
            ApplicationLogo = ApplicationConstants.ApplicationLogo,
            ApplicationName = ApplicationConstants.ApplicationName,
            LdapLogin = ApplicationConstants.LdapLogin,
            WindowsEnv = ApplicationConstants.WindowsEnv,
            ActivateAdHocQueriesModule = ApplicationConstants.ActivateAdHocQueriesModule,
            ActivateTaskSchedulerModule = ApplicationConstants.ActivateTaskSchedulerModule
        };
    }

    [Authorize]
    [HttpGet("CheckSmtpConfiguration")]
    public async Task<bool> CheckSmtpConfigurationAsync()
    {
        return await _context.SmtpConfiguration.AnyAsync(a => a.IsActivated);
    }

    [Authorize]
    [HttpGet("CheckTaskHeaderEmail")]
    public async Task<bool> CheckTaskHeaderEmailAsync(int taskHeaderId)
    {
        var email = await _context.TaskEmailRecipient
            .Where(a => a.TaskHeader!.TaskHeaderId == taskHeaderId)
            .Select(a => a.Email)
            .FirstOrDefaultAsync();

        return email != "[]" && await _context.TaskEmailRecipient
            .AnyAsync(a => a.TaskHeader.TaskHeaderId == taskHeaderId);
    }

    [Authorize]
    [HttpGet("GetUploadedFilePath")]
    public Tuple<string, string> GetUploadedFilePath(string fileName)
    {
        var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "upload");
        var filePath = Path.Combine(uploads, fileName);
        var savePath = Path.Combine("upload", fileName);
        return new Tuple<string, string>(savePath, filePath);
    }

    [Authorize]
    [HttpGet("GetApplicationParameters")]
    public async Task<ApplicationParameters?> GetApplicationParametersAsync()
    {
        return await _context.ApplicationParameters
            .OrderBy(a => a.Id)
            .FirstOrDefaultAsync();
    }
}
