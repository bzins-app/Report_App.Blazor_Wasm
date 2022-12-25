using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Services.FilesManagement;
using Report_App_WASM.Shared.ApiExchanges;

namespace Report_App_WASM.Server.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize]
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class DepositPathController : ControllerBase
    {
        private readonly ILogger<DepositPathController> _logger;
        private readonly SftpService _sftpService;
        private readonly FtpService _ftpService;
        private readonly LocalFilesService _fileService;
        private readonly ApplicationDbContext _context;

        public DepositPathController(ILogger<DepositPathController> logger,
            SftpService sftpService, FtpService ftpService, LocalFilesService fileService, ApplicationDbContext context)
        {
            _logger = logger;
            _ftpService = ftpService;
            _sftpService = sftpService;
            _fileService = fileService;
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> TestDepositPathAsync(ApiCrudPayload<DepositPathTest> value)
        {

            if (value.EntityValue!.UseSftpProtocol)
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var config = await _context.SftpConfiguration.Where(a => a!.SftpConfigurationId == value.EntityValue.SftpConfigurationId).Select(a => a.UseFtpProtocol).FirstOrDefaultAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                if (config)
                {
                    using var deposit = new FtpService(_context);
                    var result = await _ftpService.TestDirectoryAsync(value.EntityValue.SftpConfigurationId, value.EntityValue.FilePath, value.EntityValue.TryToCreateFolder);
                    return Ok(result);
                }
                else
                {
                    using var deposit = new SftpService(_context);
#pragma warning disable CS8604 // Possible null reference argument for parameter 'remoteFilePath' in 'Task<SubmitResult> SftpService.TestDirectoryAsync(int sftpconfigurationId, string remoteFilePath, bool tryCreateFolder = false)'.
                    var result = await _sftpService.TestDirectoryAsync(value.EntityValue.SftpConfigurationId, value.EntityValue.FilePath, value.EntityValue.TryToCreateFolder);
#pragma warning restore CS8604 // Possible null reference argument for parameter 'remoteFilePath' in 'Task<SubmitResult> SftpService.TestDirectoryAsync(int sftpconfigurationId, string remoteFilePath, bool tryCreateFolder = false)'.
                    return Ok(result);
                }
            }

            {
#pragma warning disable CS8604 // Possible null reference argument for parameter 'remoteFilePath' in 'Task<SubmitResult> LocalFilesService.TestDirectory(string remoteFilePath, bool tryCreateFolder = false)'.
                var result = await _fileService.TestDirectory(value.EntityValue.FilePath, value.EntityValue.TryToCreateFolder);
#pragma warning restore CS8604 // Possible null reference argument for parameter 'remoteFilePath' in 'Task<SubmitResult> LocalFilesService.TestDirectory(string remoteFilePath, bool tryCreateFolder = false)'.
                return Ok(result);
            }
        }
    }
}
