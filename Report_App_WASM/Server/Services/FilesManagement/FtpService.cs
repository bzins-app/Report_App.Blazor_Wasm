using FluentFTP;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Utils.EncryptDecrypt;
using Report_App_WASM.Shared;

namespace Report_App_WASM.Server.Services.FilesManagement
{
    public class FtpService : IDisposable
    {
        // private readonly ILogger<FtpService> _logger;
        private readonly ApplicationDbContext _context;

        public FtpService(/*ILogger<FtpService> logger, */ApplicationDbContext context)
        {
            //  _logger = logger;
            _context = context;
        }

        private async Task<SftpConfiguration?> GetSftpConfigurationAsync(int sftpconfigurationId)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.SftpConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'EntityFrameworkQueryableExtensions.AsNoTracking<TEntity>(IQueryable<TEntity>)'. Nullability of type argument 'Report_App_WASM.Server.Models.SftpConfiguration?' doesn't match 'class' constraint.
            return await _context.SftpConfiguration.Where(a => a.SftpConfigurationId == sftpconfigurationId).AsNoTracking().FirstOrDefaultAsync();
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.SftpConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'EntityFrameworkQueryableExtensions.AsNoTracking<TEntity>(IQueryable<TEntity>)'. Nullability of type argument 'Report_App_WASM.Server.Models.SftpConfiguration?' doesn't match 'class' constraint.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        public async Task<IEnumerable<FtpListItem>?> ListAllFilesAsync(int sftpconfigurationId, string remoteDirectory = ".")
        {
            var config = await GetSftpConfigurationAsync(sftpconfigurationId);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            using var client = new AsyncFtpClient(config.Host, config.UserName, EncryptDecrypt.DecryptString(config.Password));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS0168 // The variable 'exception' is declared but never used
            try
            {
                await client.Connect();
                return await client.GetListing(remoteDirectory);
            }
            catch (Exception exception)
            {
                // _logger.LogError(exception, $"Failed in listing files under [{remoteDirectory}]");
                return null;
            }
            finally
            {
                await client.Disconnect();
            }
#pragma warning restore CS0168 // The variable 'exception' is declared but never used
        }

        public async Task<SubmitResult> UploadFileAsync(int sftpconfigurationId, string localFilePath, string remoteDirectory, string fileName, bool tryCreateFolder = false)
        {
            var config = await GetSftpConfigurationAsync(sftpconfigurationId);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            using var client = new AsyncFtpClient(config.Host, config.UserName, EncryptDecrypt.DecryptString(config.Password));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            try
            {
                await client.Connect();
                if (tryCreateFolder && !await client.DirectoryExists(remoteDirectory))
                {
                    await client.CreateDirectory(remoteDirectory);
                }
                var destinationPath = Path.Combine(remoteDirectory, fileName);
                using FileStream fs = new(localFilePath, FileMode.Open);
                await client.UploadStream(fs, destinationPath); ;
                // _logger.LogInformation($"Finished uploading file [{localFilePath}] to [{remoteDirectory}]");
            }
            catch (Exception exception)
            {
                // _logger.LogError(exception, $"Failed in uploading file [{localFilePath}] to [{remoteDirectory}]");
                return new() { Success = false, Message = exception.Message };
            }
            finally
            {
                await client.Disconnect();
            }
            return new() { Success = true, Message = "Ok" };
        }

        public async Task<SubmitResult> DownloadFileAsync(int sftpconfigurationId, string remoteFilePath, string localFilePath)
        {
            var config = await GetSftpConfigurationAsync(sftpconfigurationId);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            using var client = new AsyncFtpClient(config.Host, config.UserName, EncryptDecrypt.DecryptString(config.Password));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            try
            {
                await client.Connect();
                await client.DownloadFile(remoteFilePath, localFilePath);
                //  _logger.LogInformation($"Finished downloading file [{localFilePath}] from [{remoteFilePath}]");
            }
            catch (Exception exception)
            {
                // _logger.LogError(exception, $"Failed in downloading file [{localFilePath}] from [{remoteFilePath}]");
                return new() { Success = false, Message = exception.Message };
            }
            finally
            {
                await client.Disconnect();
            }
            return new() { Success = true, Message = "Ok" };
        }

        public async Task<SubmitResult> DeleteFileAsync(int sftpconfigurationId, string remoteFilePath)
        {
            var config = await GetSftpConfigurationAsync(sftpconfigurationId);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            using var client = new AsyncFtpClient(config.Host, config.UserName, EncryptDecrypt.DecryptString(config.Password));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            try
            {
                await client.Connect();
                await client.DeleteFile(remoteFilePath);
                // _logger.LogInformation($"File [{remoteFilePath}] deleted.");
            }
            catch (Exception exception)
            {
                //  _logger.LogError(exception, $"Failed in deleting file [{remoteFilePath}]");
                return new() { Success = false, Message = exception.Message };
            }
            finally
            {
                await client.Disconnect();
            }
            return new() { Success = true, Message = "Ok" };
        }

        public async Task<SubmitResult> TestDirectoryAsync(int sftpconfigurationId, string? remoteFilePath, bool tryCreateFolder = false)
        {
            var config = await GetSftpConfigurationAsync(sftpconfigurationId);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            using var client = new AsyncFtpClient(config.Host, config.UserName, EncryptDecrypt.DecryptString(config.Password));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            bool checkAcces;
            try
            {
                await client.Connect();
                checkAcces = await client.DirectoryExists(remoteFilePath);
                if (!checkAcces && tryCreateFolder)
                {
                    await client.CreateDirectory(remoteFilePath);
                    checkAcces = await client.DirectoryExists(remoteFilePath);
                }
                //   _logger.LogInformation($"File [{remoteFilePath}] deleted.");
            }
            catch (Exception exception)
            {
                //  _logger.LogError(exception, $"Failed in deleting file [{remoteFilePath}]");
                return new() { Success = false, Message = exception.Message };
            }
            finally
            {
                await client.Disconnect();
            }
            return new() { Success = checkAcces, Message = checkAcces == false ? "Cannot reach the path" : "Ok" };
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
