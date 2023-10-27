using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Services.BackgroundWorker;
using Report_App_WASM.Shared;

namespace Report_App_WASM.Server.Services.FilesManagement;

public class LocalFilesService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public LocalFilesService(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment)
    {
        _context = context;
        _hostingEnvironment = hostingEnvironment;
    }

    public async Task<SubmitResult> SaveFileForBackupAsync(MemoryFileContainer file, string fileName)
    {
        try
        {
            var storagePath = Path.Combine(_hostingEnvironment.WebRootPath, "docsstorage");
            var filePath = Path.Combine(storagePath, fileName);
            await using var fileWriter = new FileStream(filePath, FileMode.CreateNew);
            await fileWriter.WriteAsync(file.Content);
        }
        catch (Exception ex)
        {
            return new SubmitResult { Success = false, Message = ex.Message };
        }

        return new SubmitResult { Success = true, Message = "Ok" };
    }

    public async Task<SubmitResult> SaveFileAsync(MemoryFileContainer file, string fileName, string storagePath,
        bool tryCreateFolder = false)
    {
        try
        {
            var filePath = Path.Combine(storagePath, fileName);
            var checkAcces = Directory.Exists(storagePath);

            if (!checkAcces && tryCreateFolder)
            {
                var t = Directory.CreateDirectory(storagePath);
                checkAcces = t.Exists;
            }

            if (checkAcces)
            {
                await using var fileWriter = new FileStream(filePath, FileMode.CreateNew);
                await fileWriter.WriteAsync(file.Content);
            }
            else
            {
                throw new DirectoryNotFoundException();
            }
        }
        catch (Exception ex)
        {
            return new SubmitResult { Success = false, Message = ex.Message };
        }

        return new SubmitResult { Success = true, Message = "Ok" };
    }


    public FileInfo GetFileInfo(string? filePath)
    {
        var storagePath = Path.Combine(_hostingEnvironment.WebRootPath, filePath!);
        return new FileInfo(storagePath);
    }

    public async Task RemoveLocalFilesAsync(List<ApplicationLogReportResult> filesInfo)
    {
        var storagePath = Path.Combine(_hostingEnvironment.WebRootPath, "docsstorage");

        var direct = new DirectoryInfo(storagePath);
        if (direct.Exists)
        {
            foreach (var fi in direct.EnumerateFiles())
                if (filesInfo.Any(a => a.FileName == fi.Name))
                {
                    filesInfo.Where(a => a.FileName == fi.Name).All(a => a.IsAvailable = false);
                    fi.Delete();
                }
        }
        else
        {
            filesInfo.All(file => file.IsAvailable = false);
        }

        _context.UpdateRange(filesInfo);
        await _context.SaveChangesAsync();
    }

    public Task<SubmitResult> TestDirectory(string remoteFilePath, bool tryCreateFolder = false)
    {
        bool checkAcces;
        try
        {
            checkAcces = Directory.Exists(remoteFilePath);

            if (!checkAcces && tryCreateFolder)
            {
                var t = Directory.CreateDirectory(remoteFilePath);
                checkAcces = t.Exists;
            }
        }
        catch (Exception ex)
        {
            return Task.FromResult(new SubmitResult { Success = false, Message = ex.Message });
        }

        return Task.FromResult(new SubmitResult
            { Success = checkAcces, Message = checkAcces == false ? "Cannot reach the path" : "Ok" });
    }
}