using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Shared;
using ReportAppWASM.Server.Services.FilesManagement;

namespace Report_App_WASM.Server.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize]
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class DepositPathController : ControllerBase
    {
        private readonly ILogger<DepositPathController> _logger;
        private readonly SftpService _SftpService;
        private readonly FtpService _FtpService;
        private readonly LocalFilesService _FileService;
        private readonly ApplicationDbContext _context;

        public DepositPathController(ILogger<DepositPathController> logger,
            SftpService SftpService, FtpService FtpService, LocalFilesService FileService, ApplicationDbContext context)
        {
            _logger = logger;
            _FtpService = FtpService;
            _SftpService = SftpService;
            _FileService = FileService;
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> TestDepositPathAsync(ApiCRUDPayload<DepositPathTest> value)
        {           

            if (value.EntityValue.UseSFTPProtocol)
            {
                var config = await _context.SFTPConfiguration.Where(a => a.SFTPConfigurationId == value.EntityValue.SFTPConfigurationId).Select(a => a.UseFTPProtocol).FirstOrDefaultAsync();
                if (config)
                {
                    using var deposit = new FtpService(_context);
                    var result = await _FtpService.TestDirectoryAsync(value.EntityValue.SFTPConfigurationId, value.EntityValue.FilePath, value.EntityValue.TryToCreateFolder);
                    return Ok(result);
                }
                else
                {
                    using var deposit = new SftpService(_context);
                    var result = await _SftpService.TestDirectoryAsync(value.EntityValue.SFTPConfigurationId, value.EntityValue.FilePath, value.EntityValue.TryToCreateFolder);
                    return Ok(result);
                }
            }
            else
            {
                var result = await _FileService.TestDirectory(value.EntityValue.FilePath, value.EntityValue.TryToCreateFolder);
                return Ok(result);
            }
        }
    }
}
