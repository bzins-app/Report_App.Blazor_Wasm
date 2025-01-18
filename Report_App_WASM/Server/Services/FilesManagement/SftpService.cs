using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace Report_App_WASM.Server.Services.FilesManagement;

public class SftpService : IDisposable
{
    // private readonly ILogger<FtpService> _logger;
    private readonly ApplicationDbContext _context;

    public SftpService( /*ILogger<FtpService> logger, */ ApplicationDbContext context)
    {
        //  _logger = logger;
        _context = context;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private async Task<FileStorageConfiguration> GetSftpConfigurationAsync(long sftpconfigurationId)
    {
        return (await _context.FileStorageConfiguration.Where(a => a.FileStorageConfigurationId == sftpconfigurationId).AsNoTracking()
            .FirstOrDefaultAsync())!;
    }

    public async Task<IEnumerable<ISftpFile>?> ListAllFilesAsync(int sftpconfigurationId, string remoteDirectory = ".")
    {
        var config = await GetSftpConfigurationAsync(sftpconfigurationId);
        using var client = new SftpClient(config.Host, config.Port == 0 ? 22 : config.Port, config.UserName,
            EncryptDecrypt.DecryptString(config.Password));
        try
        {
            client.Connect();
            return client.ListDirectory(remoteDirectory);
        }
        catch (Exception)
        {
            //  _logger.LogError(exception, $"Failed in listing files under [{remoteDirectory}]");
            return null;
        }
        finally
        {
            client.Disconnect();
        }
    }

    public async Task<SubmitResult> UploadFileAsync(long sftpconfigurationId, string localFilePath,
        string remoteDirectory, string fileName, bool tryCreateFolder = false)
    {
        var config = await GetSftpConfigurationAsync(sftpconfigurationId);
        using var client = new SftpClient(config.Host, config.Port == 0 ? 22 : config.Port, config.UserName,
            EncryptDecrypt.DecryptString(config.Password));
        try
        {
            client.Connect();
            if (tryCreateFolder && !client.Exists(remoteDirectory)) client.CreateDirectory(remoteDirectory);
            var destinationPath = Path.Combine(remoteDirectory, fileName);
            await using FileStream fs = new(localFilePath, FileMode.Open);
            client.BufferSize = 4 * 1024;
            client.UploadFile(fs, destinationPath);
            // _logger.LogInformation($"Finished uploading file [{localFilePath}] to [{remoteDirectory}]");
        }
        catch (Exception exception)
        {
            // _logger.LogError(exception, $"Failed in uploading file [{localFilePath}] to [{remoteDirectory}]");
            return new SubmitResult { Success = false, Message = exception.Message };
        }
        finally
        {
            client.Disconnect();
        }

        return new SubmitResult { Success = true, Message = "Ok" };
    }

    public async Task<SubmitResult> DownloadFileAsync(int sftpconfigurationId, string remoteFilePath,
        string localFilePath)
    {
        var config = await GetSftpConfigurationAsync(sftpconfigurationId);
        using var client = new SftpClient(config.Host, config.Port == 0 ? 22 : config.Port, config.UserName,
            EncryptDecrypt.DecryptString(config.Password));
        try
        {
            client.Connect();
            await using var s = File.Create(localFilePath);
            client.DownloadFile(remoteFilePath, s);
            //  _logger.LogInformation($"Finished downloading file [{localFilePath}] from [{remoteFilePath}]");
        }
        catch (Exception exception)
        {
            //  _logger.LogError(exception, $"Failed in downloading file [{localFilePath}] from [{remoteFilePath}]");
            return new SubmitResult { Success = false, Message = exception.Message };
        }
        finally
        {
            client.Disconnect();
        }

        return new SubmitResult { Success = true, Message = "Ok" };
    }

    public async Task<SubmitResult> DeleteFileAsync(int sftpconfigurationId, string remoteFilePath)
    {
        var config = await GetSftpConfigurationAsync(sftpconfigurationId);
        using var client = new SftpClient(config.Host, config.Port == 0 ? 22 : config.Port, config.UserName,
            EncryptDecrypt.DecryptString(config.Password));
        try
        {
            client.Connect();
            client.DeleteFile(remoteFilePath);
            //   _logger.LogInformation($"File [{remoteFilePath}] deleted.");
        }
        catch (Exception exception)
        {
            //  _logger.LogError(exception, $"Failed in deleting file [{remoteFilePath}]");
            return new SubmitResult { Success = false, Message = exception.Message };
        }
        finally
        {
            client.Disconnect();
        }

        return new SubmitResult { Success = true, Message = "Ok" };
    }



    public async Task<SubmitResult> DeleteDirectoryFilesAsync(int sftpconfigurationId, string remoteFilePath)
    {
        var config = await GetSftpConfigurationAsync(sftpconfigurationId);
        using var client = new SftpClient(config.Host, config.Port == 0 ? 22 : config.Port, config.UserName,
            EncryptDecrypt.DecryptString(config.Password));
        try
        {
            client.Connect();
            if (!client.Exists(remoteFilePath))
                return new SubmitResult { Success = true, Message = "Ok" };

            foreach (var file in client.ListDirectory(remoteFilePath))
            {
                if (file.Name.Equals(".") || file.Name.Equals(".."))
                    continue;

                    client.DeleteFile(file.FullName);
            }
            //   _logger.LogInformation($"File [{remoteFilePath}] deleted.");
        }
        catch (Exception exception)
        {
            //  _logger.LogError(exception, $"Failed in deleting file [{remoteFilePath}]");
            return new SubmitResult { Success = false, Message = exception.Message };
        }
        finally
        {
            client.Disconnect();
        }

        return new SubmitResult { Success = true, Message = "Ok" };
    }

    public async Task<SubmitResult> TestDirectoryAsync(int sftpconfigurationId, string remoteFilePath,
        bool tryCreateFolder = false)
    {
        var config = await GetSftpConfigurationAsync(sftpconfigurationId);
        using var client = new SftpClient(config.Host, config.Port == 0 ? 22 : config.Port, config.UserName,
            EncryptDecrypt.DecryptString(config.Password));
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
            return new SubmitResult { Success = false, Message = exception.Message };
        }
        finally
        {
            client.Disconnect();
        }

        return new SubmitResult
            { Success = checkAcces, Message = checkAcces == false ? "Cannot reach the path" : "Ok" };
    }
}