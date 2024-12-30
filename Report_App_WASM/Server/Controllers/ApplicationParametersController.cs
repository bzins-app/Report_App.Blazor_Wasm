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
    public async Task<IEnumerable<SelectItemDataproviderInfo>> GetActivitiesInfo()
    {
        return await _context.DataProvider
            .Where(a => a.ProviderType == Shared.ProviderType.SourceDatabase)
            .AsNoTracking()
            .Select(( a) => new SelectItemDataproviderInfo
            {
                DataProviderId = a.DataProviderId,
                ProviderName = a.ProviderName,
                HasALogo = !string.IsNullOrEmpty(a.ProviderIcon),
                IsVisible = a.IsVisible,
                LogoPath = a.ProviderIcon,
                IsActivated = a.IsEnabled,
                DatabaseConnectionId = a.DatabaseConnections.FirstOrDefault().DatabaseConnectionId
            })
            .ToArrayAsync();
    }

    [Authorize]
    [HttpGet("DataTransfers")]
    public async Task<IEnumerable<SelectItemDataproviderInfo>> GetDataTransfersInfo()
    {
        return await _context.DataProvider
            .Where(a => a.ProviderType == Shared.ProviderType.TargetDatabase)
            .AsNoTracking()
            .Select(( a) => new SelectItemDataproviderInfo
            {
                DataProviderId = a.DataProviderId,
                ProviderName = a.ProviderName,
                HasALogo = !string.IsNullOrEmpty(a.ProviderIcon),
                IsVisible = a.IsVisible,
                LogoPath = a.ProviderIcon,
                IsActivated = a.IsEnabled,
                DatabaseConnectionId = a.DatabaseConnections.FirstOrDefault().DatabaseConnectionId
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
        return await _context.FileStorageLocation
            .Select(a => new SelectItem { Id = a.FileStorageLocationId, Name = a.ConfigurationName })
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
        var email = await _context.ScheduledTaskDistributionList
            .Where(a => a.ScheduledTask!.ScheduledTaskId == taskHeaderId)
            .Select(a => a.Recipients)
            .FirstOrDefaultAsync();

        return email != "[]" && await _context.ScheduledTaskDistributionList
            .AnyAsync(a => a.ScheduledTask.ScheduledTaskId == taskHeaderId);
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
    public async Task<SystemParameters?> GetApplicationParametersAsync()
    {
        return await _context.SystemParameters
            .OrderBy(a => a.Id)
            .FirstOrDefaultAsync();
    }
}