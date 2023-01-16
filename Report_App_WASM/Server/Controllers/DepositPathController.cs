using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Services.FilesManagement;
using Report_App_WASM.Shared.ApiExchanges;

namespace Report_App_WASM.Server.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
[Route("api/[controller]/[Action]")]
[ApiController]
public class DepositPathController : ControllerBase, IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly LocalFilesService _fileService;
    private readonly FtpService _ftpService;
    private readonly ILogger<DepositPathController> _logger;
    private readonly SftpService _sftpService;

    public DepositPathController(ILogger<DepositPathController> logger,
        SftpService sftpService, FtpService ftpService, LocalFilesService fileService, ApplicationDbContext context)
    {
        _logger = logger;
        _ftpService = ftpService;
        _sftpService = sftpService;
        _fileService = fileService;
        _context = context;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    [HttpPost]
    public async Task<IActionResult> TestDepositPathAsync(ApiCrudPayload<DepositPathTest> value)
    {
        if (value.EntityValue!.UseSftpProtocol&& value.EntityValue.SftpConfigurationId>0)
        {
            var config = await _context.SftpConfiguration
                .Where(a => a.SftpConfigurationId == value.EntityValue.SftpConfigurationId)
                .Select(a => a.UseFtpProtocol).FirstOrDefaultAsync();

            if (config)
            {
                using var deposit = new FtpService(_context);
                var result = await _ftpService.TestDirectoryAsync(value.EntityValue.SftpConfigurationId,
                    value.EntityValue.FilePath, value.EntityValue.TryToCreateFolder);
                return Ok(result);
            }
            else
            {
                using var deposit = new SftpService(_context);
                var result = await _sftpService.TestDirectoryAsync(value.EntityValue.SftpConfigurationId,
                    value.EntityValue.FilePath!, value.EntityValue.TryToCreateFolder);
                return Ok(result);
            }
        }

        {
            var result =
                await _fileService.TestDirectory(value.EntityValue.FilePath!, value.EntityValue.TryToCreateFolder);
            return Ok(result);
        }
    }
}