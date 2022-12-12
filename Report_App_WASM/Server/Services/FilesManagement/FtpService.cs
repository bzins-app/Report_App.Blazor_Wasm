using FluentFTP;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Utils.EncryptDecrypt;
using Report_App_WASM.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ReportAppWASM.Server.Services.FilesManagement
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

        private async Task<SFTPConfiguration> GetSFTPConfigurationAsync(int SFTPconfigurationId)
        {
            return await _context.SFTPConfiguration.Where(a => a.SFTPConfigurationId == SFTPconfigurationId).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<FtpListItem>> ListAllFilesAsync(int SFTPconfigurationId, string remoteDirectory = ".")
        {
            var _config = await GetSFTPConfigurationAsync(SFTPconfigurationId);
            using var client = new AsyncFtpClient(_config.Host, _config.UserName, EncryptDecrypt.DecryptString(_config.Password));
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
        }

        public async Task<SubmitResult> UploadFileAsync(int SFTPconfigurationId, string localFilePath, string remoteDirectory, string fileName, bool tryCreateFolder = false)
        {
            var _config = await GetSFTPConfigurationAsync(SFTPconfigurationId);
            using var client = new AsyncFtpClient(_config.Host, _config.UserName, EncryptDecrypt.DecryptString(_config.Password));
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
                return new SubmitResult() { Success = false, Message = exception.Message };
            }
            finally
            {
                await client.Disconnect();
            }
            return new SubmitResult() { Success = true, Message = "Ok" };
        }

        public async Task<SubmitResult> DownloadFileAsync(int SFTPconfigurationId, string remoteFilePath, string localFilePath)
        {
            var _config = await GetSFTPConfigurationAsync(SFTPconfigurationId);
            using var client = new AsyncFtpClient(_config.Host, _config.UserName, EncryptDecrypt.DecryptString(_config.Password));
            try
            {
                await client.Connect();
                await client.DownloadFile(remoteFilePath, localFilePath);
                //  _logger.LogInformation($"Finished downloading file [{localFilePath}] from [{remoteFilePath}]");
            }
            catch (Exception exception)
            {
                // _logger.LogError(exception, $"Failed in downloading file [{localFilePath}] from [{remoteFilePath}]");
                return new SubmitResult() { Success = false, Message = exception.Message };
            }
            finally
            {
                await client.Disconnect();
            }
            return new SubmitResult() { Success = true, Message = "Ok" };
        }

        public async Task<SubmitResult> DeleteFileAsync(int SFTPconfigurationId, string remoteFilePath)
        {
            var _config = await GetSFTPConfigurationAsync(SFTPconfigurationId);
            using var client = new AsyncFtpClient(_config.Host, _config.UserName, EncryptDecrypt.DecryptString(_config.Password));
            try
            {
                await client.Connect();
                await client.DeleteFile(remoteFilePath);
                // _logger.LogInformation($"File [{remoteFilePath}] deleted.");
            }
            catch (Exception exception)
            {
                //  _logger.LogError(exception, $"Failed in deleting file [{remoteFilePath}]");
                return new SubmitResult() { Success = false, Message = exception.Message };
            }
            finally
            {
                await client.Disconnect();
            }
            return new SubmitResult() { Success = true, Message = "Ok" };
        }

        public async Task<SubmitResult> TestDirectoryAsync(int SFTPconfigurationId, string remoteFilePath, bool tryCreateFolder = false)
        {
            var _config = await GetSFTPConfigurationAsync(SFTPconfigurationId);
            using var client = new AsyncFtpClient(_config.Host, _config.UserName, EncryptDecrypt.DecryptString(_config.Password));
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
                return new SubmitResult() { Success = false, Message = exception.Message };
            }
            finally
            {
                await client.Disconnect();
            }
            return new SubmitResult() { Success = checkAcces, Message = checkAcces == false ? "Cannot reach the path" : "Ok" };
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
