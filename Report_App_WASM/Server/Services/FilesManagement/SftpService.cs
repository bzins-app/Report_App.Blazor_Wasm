using Microsoft.EntityFrameworkCore;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Utils.EncryptDecrypt;
using Report_App_WASM.Shared;

namespace Report_App_WASM.Server.Services.FilesManagement
{

    public class SftpService : IDisposable
    {
        // private readonly ILogger<FtpService> _logger;
        private readonly ApplicationDbContext _context;

        public SftpService(/*ILogger<FtpService> logger, */ApplicationDbContext context)
        {
            //  _logger = logger;
            _context = context;
        }

        private async Task<SftpConfiguration> GetSftpConfigurationAsync(int sftpconfigurationId)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.SftpConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'EntityFrameworkQueryableExtensions.AsNoTracking<TEntity>(IQueryable<TEntity>)'. Nullability of type argument 'Report_App_WASM.Server.Models.SftpConfiguration?' doesn't match 'class' constraint.
            return await _context.SftpConfiguration.Where(a => a.SftpConfigurationId == sftpconfigurationId).AsNoTracking().FirstOrDefaultAsync();
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.SftpConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'EntityFrameworkQueryableExtensions.AsNoTracking<TEntity>(IQueryable<TEntity>)'. Nullability of type argument 'Report_App_WASM.Server.Models.SftpConfiguration?' doesn't match 'class' constraint.
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        public async Task<IEnumerable<SftpFile>?> ListAllFilesAsync(int sftpconfigurationId, string remoteDirectory = ".")
        {
            var config = await GetSftpConfigurationAsync(sftpconfigurationId);
            using var client = new SftpClient(config.Host, config.Port == 0 ? 22 : config.Port, config.UserName, EncryptDecrypt.DecryptString(config.Password));
#pragma warning disable CS0168 // The variable 'exception' is declared but never used
            try
            {
                client.Connect();
                return client.ListDirectory(remoteDirectory);
            }
            catch (Exception exception)
            {
                //  _logger.LogError(exception, $"Failed in listing files under [{remoteDirectory}]");
                return null;
            }
            finally
            {
                client.Disconnect();
            }
#pragma warning restore CS0168 // The variable 'exception' is declared but never used
        }

        public async Task<SubmitResult> UploadFileAsync(int sftpconfigurationId, string localFilePath, string remoteDirectory, string fileName, bool tryCreateFolder = false)
        {
            var config = await GetSftpConfigurationAsync(sftpconfigurationId);
            using var client = new SftpClient(config.Host, config.Port == 0 ? 22 : config.Port, config.UserName, EncryptDecrypt.DecryptString(config.Password));
            try
            {
                client.Connect();
                if (tryCreateFolder && !client.Exists(remoteDirectory))
                {
                    client.CreateDirectory(remoteDirectory);
                }
                var destinationPath = Path.Combine(remoteDirectory, fileName);
                using FileStream fs = new(localFilePath, FileMode.Open);
                client.BufferSize = 4 * 1024;
                client.UploadFile(fs, destinationPath);
                // _logger.LogInformation($"Finished uploading file [{localFilePath}] to [{remoteDirectory}]");
            }
            catch (Exception exception)
            {
                // _logger.LogError(exception, $"Failed in uploading file [{localFilePath}] to [{remoteDirectory}]");
                return new() { Success = false, Message = exception.Message };
            }
            finally
            {
                client.Disconnect();
            }
            return new() { Success = true, Message = "Ok" };
        }

        public async Task<SubmitResult> DownloadFileAsync(int sftpconfigurationId, string remoteFilePath, string localFilePath)
        {
            var config = await GetSftpConfigurationAsync(sftpconfigurationId);
            using var client = new SftpClient(config.Host, config.Port == 0 ? 22 : config.Port, config.UserName, EncryptDecrypt.DecryptString(config.Password));
            try
            {
                client.Connect();
                using var s = File.Create(localFilePath);
                client.DownloadFile(remoteFilePath, s);
                //  _logger.LogInformation($"Finished downloading file [{localFilePath}] from [{remoteFilePath}]");
            }
            catch (Exception exception)
            {
                //  _logger.LogError(exception, $"Failed in downloading file [{localFilePath}] from [{remoteFilePath}]");
                return new() { Success = false, Message = exception.Message };
            }
            finally
            {
                client.Disconnect();
            }
            return new() { Success = true, Message = "Ok" };
        }

        public async Task<SubmitResult> DeleteFileAsync(int sftpconfigurationId, string remoteFilePath)
        {
            var config = await GetSftpConfigurationAsync(sftpconfigurationId);
            using var client = new SftpClient(config.Host, config.Port == 0 ? 22 : config.Port, config.UserName, EncryptDecrypt.DecryptString(config.Password));
            try
            {
                client.Connect();
                client.DeleteFile(remoteFilePath);
                //   _logger.LogInformation($"File [{remoteFilePath}] deleted.");
            }
            catch (Exception exception)
            {
                //  _logger.LogError(exception, $"Failed in deleting file [{remoteFilePath}]");
                return new() { Success = false, Message = exception.Message };
            }
            finally
            {
                client.Disconnect();
            }
            return new() { Success = true, Message = "Ok" };
        }

        public async Task<SubmitResult> TestDirectoryAsync(int sftpconfigurationId, string remoteFilePath, bool tryCreateFolder = false)
        {
            var config = await GetSftpConfigurationAsync(sftpconfigurationId);
            using var client = new SftpClient(config.Host, config.Port == 0 ? 22 : config.Port, config.UserName, EncryptDecrypt.DecryptString(config.Password));
            bool checkAcces;
            try
            {
                client.Connect();
                checkAcces = client.Exists(remoteFilePath);
                if (!checkAcces && tryCreateFolder)
                {
                    client.CreateDirectory(remoteFilePath);
                    checkAcces = client.Exists(remoteFilePath);
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
                client.Disconnect();
            }
            return new() { Success = checkAcces, Message = checkAcces == false ? "Cannot reach the path" : "Ok" };
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

}
