using Microsoft.AspNetCore.Mvc;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Shared;

namespace ReportAppWASM.Server.Services.FilesManagement
{
    public class LocalFilesService
    {
        readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public LocalFilesService(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<SubmitResult> SaveFileForBackupAsync(FileContentResult file, string fileName)
        {
            try
            {
                var StoragePath = Path.Combine(_hostingEnvironment.WebRootPath, "docsstorage");
                var filePath = Path.Combine(StoragePath, fileName);
                await using var fileWriter = new FileStream(filePath, FileMode.CreateNew);
                await fileWriter.WriteAsync(file.FileContents);
            }
            catch (Exception ex)
            {
                return new SubmitResult() { Success = false, Message = ex.Message };
            }
            return new SubmitResult() { Success = true, Message = "Ok" };
        }

        public async Task<SubmitResult> SaveFileAsync(FileContentResult file, string fileName, string storagePath, bool tryCreateFolder = false)
        {
            try
            {
                var filePath = Path.Combine(storagePath, fileName);
                bool checkAcces = Directory.Exists(storagePath);

                if (!checkAcces && tryCreateFolder)
                {
                    var t = Directory.CreateDirectory(storagePath);
                    checkAcces = t.Exists;
                }

                if (checkAcces)
                {
                    await using var fileWriter = new FileStream(filePath, FileMode.CreateNew);
                    await fileWriter.WriteAsync(file.FileContents);
                }
                else throw new DirectoryNotFoundException();
            }
            catch (Exception ex)
            {
                return new SubmitResult() { Success = false, Message = ex.Message };
            }
            return new SubmitResult() { Success = true, Message = "Ok" };
        }


        public FileInfo GetFileInfo(string filePath)
        {
            var StoragePath = Path.Combine(_hostingEnvironment.WebRootPath, filePath);
            return new FileInfo(StoragePath);
        }

        public async Task RemoveLocalFilesAsync(List<ApplicationLogReportResult> filesInfo)
        {
            var StoragePath = Path.Combine(_hostingEnvironment.WebRootPath, "docsstorage");

            var direct = new DirectoryInfo(StoragePath);
            if (direct.Exists)
            {
                foreach (FileInfo fi in direct.EnumerateFiles())
                {
                    if (filesInfo.Any(a => a.FileName == fi.Name))
                    {
                        filesInfo.Where(a => a.FileName == fi.Name).All(a => a.IsAvailable = false);
                        fi.Delete();
                    }
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
                return Task.FromResult(new SubmitResult() { Success = false, Message = ex.Message });
            }

            return Task.FromResult(new SubmitResult() { Success = checkAcces, Message = checkAcces == false ? "Cannot reach the path" : "Ok" });
        }
    }
}
