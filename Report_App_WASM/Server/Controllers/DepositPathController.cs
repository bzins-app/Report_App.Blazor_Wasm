﻿using Report_App_WASM.Server.Services.FilesManagement;

namespace Report_App_WASM.Server.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
[Route("api/[controller]/[Action]")]
[ApiController]
public class DepositPathController : ControllerBase, IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly LocalFilesService _fileService;
    private readonly ILogger<DepositPathController> _logger;

    public DepositPathController(ILogger<DepositPathController> logger, LocalFilesService fileService,
        ApplicationDbContext context)
    {
        _logger = logger;
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
        if (value.EntityValue == null)
        {
            return BadRequest("EntityValue cannot be null.");
        }

        if (value.EntityValue.UseSftpProtocol && value.EntityValue.SftpConfigurationId > 0)
        {
            var useFtpProtocol = await _context.SftpConfiguration
                .Where(a => a.SftpConfigurationId == value.EntityValue.SftpConfigurationId)
                .Select(a => a.UseFtpProtocol)
                .FirstOrDefaultAsync();

            if (useFtpProtocol)
            {
                using var deposit = new FtpService(_context);
                var result = await deposit.TestDirectoryAsync(value.EntityValue.SftpConfigurationId,
                    value.EntityValue.FilePath, value.EntityValue.TryToCreateFolder);
                return Ok(result);
            }
            else
            {
                using var deposit = new SftpService(_context);
                var result = await deposit.TestDirectoryAsync(value.EntityValue.SftpConfigurationId,
                    value.EntityValue.FilePath!, value.EntityValue.TryToCreateFolder);
                return Ok(result);
            }
        }
        else
        {
            var result =
                await _fileService.TestDirectory(value.EntityValue.FilePath!, value.EntityValue.TryToCreateFolder);
            return Ok(result);
        }
    }
}