using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Shared;

namespace Report_App_WASM.Server.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
[Route("api/[controller]/[Action]")]
[ApiController]
public class FilesController : ControllerBase, IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly ILogger<FilesController> _logger;

    public FilesController(ILogger<FilesController> logger,
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

    [HttpPost]
    public async Task<IActionResult> UploadAsync(IFormFile file)
    {
        var returnedFiledPath = "";
        if (file.Length > 0)
        {
            var filePath = GetUploadedFilePath(file.FileName);
            returnedFiledPath = filePath.Item1;
            using var stream = System.IO.File.Create(filePath.Item2);
            await file.CopyToAsync(stream);
        }
        // Process uploaded files
        // Don't rely on or trust the FileName property without validation.

        return Ok(new SubmitResult { Success = true, Message = returnedFiledPath });
    }


    private Tuple<string, string> GetUploadedFilePath(string fileName)
    {
        var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "upload");
        var filePath = Path.Combine(uploads, fileName);
        var savePath = "upload/" + fileName;
        Tuple<string, string> result = new(savePath, filePath);
        return result;
    }
}