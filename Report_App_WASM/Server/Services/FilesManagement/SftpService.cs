using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace Report_App_WASM.Server.Services.FilesManagement;

public class SftpService : IDisposable
{
    private readonly ILogger<SftpService> _logger;
    private readonly ApplicationDbContext _context;

    public SftpService( ILogger<SftpService> logger,ApplicationDbContext context)
    {
        _context = context;
        _logger = logger;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private async Task<FileStorageConfiguration> GetSftpConfigurationAsync(long sftpconfigurationId)
    {
        return (await _context.FileStorageConfiguration.Where(a => a.FileStorageConfigurationId == sftpconfigurationId)
            .AsNoTracking()
            .FirstOrDefaultAsync())!;
    }

    public async Task<IEnumerable<ISftpFile>?> ListAllFilesAsync(int sftpconfigurationId, string remoteDirectory = ".")
    {
        var config = await GetSftpConfigurationAsync(sftpconfigurationId);
        using var client = new SftpClient(config.Host ?? string.Empty, config.Port == 0 ? 22 : config.Port,
            config.UserName ?? string.Empty,
            EncryptDecrypt.DecryptString(config.Password));
        try
        {
            client.Connect();
            return client.ListDirectory(remoteDirectory);
        }
        catch (Exception exception)
        { 
            _logger.LogError(exception.Message, $"Failed in listing files under [{remoteDirectory}]");
            _logger.LogError(exception.InnerException.Message);
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
        using var client = new SftpClient(config.Host ?? string.Empty, config.Port == 0 ? 22 : config.Port,
            config.UserName ?? string.Empty,
            EncryptDecrypt.DecryptString(config.Password));
        try
        {
            client.Connect();
            if (tryCreateFolder && !client.Exists(remoteDirectory)) client.CreateDirectory(remoteDirectory);
            var destinationPath = Path.Combine(remoteDirectory, fileName);
            await using FileStream fs = new(localFilePath, FileMode.Open);
            client.BufferSize = 4 * 1024;
            client.UploadFile(fs, destinationPath);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Failed in uploading file [{localFilePath}] to [{remoteDirectory}]");
            _logger.LogError(exception.InnerException.Message);
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
        using var client = new SftpClient(config.Host ?? string.Empty, config.Port == 0 ? 22 : config.Port,
            config.UserName ?? string.Empty,
            EncryptDecrypt.DecryptString(config.Password));
        try
        {
            client.Connect();
            await using var s = File.Create(localFilePath);
            client.DownloadFile(remoteFilePath, s);
        }
        catch (Exception exception)
        {
             _logger.LogError(exception, $"Failed in downloading file [{localFilePath}] from [{remoteFilePath}]");
             _logger.LogError(exception.InnerException.Message);
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
        using var client = new SftpClient(config.Host ?? string.Empty, config.Port == 0 ? 22 : config.Port,
            config.UserName ?? string.Empty,
            EncryptDecrypt.DecryptString(config.Password));
        try
        {
            client.Connect();
            client.DeleteFile(remoteFilePath);
        }
        catch (Exception exception)
        {
             _logger.LogError(exception, $"Failed in deleting file [{remoteFilePath}]");
             _logger.LogError(exception.InnerException.Message);
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
        using var client = new SftpClient(config.Host ?? string.Empty, config.Port == 0 ? 22 : config.Port,
            config.UserName ?? string.Empty,
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
        }
        catch (Exception exception)
        {
             _logger.LogError(exception, $"Failed in deleting file [{remoteFilePath}]");
             _logger.LogError(exception.InnerException.Message);
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