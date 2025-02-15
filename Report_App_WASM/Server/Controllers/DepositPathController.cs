using Report_App_WASM.Server.Services.FilesManagement;

namespace Report_App_WASM.Server.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
[Route("api/[controller]/[Action]")]
[ApiController]
public class DepositPathController : ControllerBase, IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly LocalFilesService _fileService;
    private readonly SftpService _sftp;
    private readonly FtpService _ftp;

    public DepositPathController(ILogger<DepositPathController> logger, LocalFilesService fileService,
        ApplicationDbContext context, SftpService sftp, FtpService ftp)
    {
        _fileService = fileService;
        _context = context;
        _sftp = sftp;
        _ftp = ftp;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    [HttpPost]
    public async Task<IActionResult> TestDepositPathAsync(ApiCrudPayload<DepositPathTest> value,  CancellationToken ct)
    {
        if (value.EntityValue == null)
        {
            return BadRequest("EntityValue cannot be null.");
        }

        if (value.EntityValue.UseSftpProtocol && value.EntityValue.SftpConfigurationId > 0)
        {
            var useFtpProtocol = await _context.FileStorageConfiguration
                .Where(a => a.FileStorageConfigurationId == value.EntityValue.SftpConfigurationId)
                .Select(a => a.ConfigurationType == FileStorageConfigurationType.FTP||a.ConfigurationType == FileStorageConfigurationType.FTPs)
                .FirstOrDefaultAsync();

            if (useFtpProtocol)
            {
                using var deposit = _ftp;
                var result = await deposit.TestDirectoryAsync(value.EntityValue.SftpConfigurationId,
                    value.EntityValue.FilePath, value.EntityValue.TryToCreateFolder,ct);
                return Ok(result);
            }
            else
            {
                using var deposit = _sftp;
                var result = await deposit.TestDirectoryAsync(value.EntityValue.SftpConfigurationId,
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