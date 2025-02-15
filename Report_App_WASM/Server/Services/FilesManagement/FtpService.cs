using FluentFTP;

namespace Report_App_WASM.Server.Services.FilesManagement;

public class FtpService : IDisposable
{
    private readonly ILogger<FtpService> _logger;
    private readonly ApplicationDbContext _context;

    public FtpService(ILogger<FtpService> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private async Task<FileStorageConfiguration?> GetSftpConfigurationAsync(long sftpconfigurationId)
    {
        return await _context.FileStorageConfiguration
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.FileStorageConfigurationId == sftpconfigurationId);
    }

    private async Task<AsyncFtpClient> GetClientAsync(long sftpconfigurationId)
    {
        var config = await GetSftpConfigurationAsync(sftpconfigurationId);
        var client = string.IsNullOrEmpty(config.UserName)
            ? new AsyncFtpClient(config.Host)
            : new AsyncFtpClient(config.Host, config.UserName, EncryptDecrypt.DecryptString(config.Password));

        if (config.ConfigurationType == FileStorageConfigurationType.FTPs)
        {
            client.Config.EncryptionMode = FtpEncryptionMode.Auto;
            client.Config.ValidateAnyCertificate = true;
        }

        return client;
    }

    private async Task<SubmitResult> ExecuteFtpOperationAsync(long sftpconfigurationId,
        Func<AsyncFtpClient, Task> operation)
    {
        using var client = await GetClientAsync(sftpconfigurationId);

        try
        {
            await client.Connect();
            await operation(client);
            return new SubmitResult { Success = true, Message = "Ok" };
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "FTP operation failed");
            return new SubmitResult { Success = false, Message = exception.Message };
        }
        finally
        {
            await client.Disconnect();
        }
    }

    public async Task<IEnumerable<FtpListItem>?> ListAllFilesAsync(int sftpconfigurationId,
        string remoteDirectory = ".")
    {
        using var client = await GetClientAsync(sftpconfigurationId);

        try
        {
            await client.Connect();
            return await client.GetListing(remoteDirectory);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Failed in listing files under [{remoteDirectory}]");
            return null;
        }
        finally
        {
            await client.Disconnect();
        }
    }

    public async Task<SubmitResult> UploadFileAsync(long sftpconfigurationId, string localFilePath,
        string remoteDirectory, string fileName, bool tryCreateFolder = false)
    {
        return await ExecuteFtpOperationAsync(sftpconfigurationId, async client =>
        {
            if (tryCreateFolder && !string.IsNullOrEmpty(remoteDirectory) &&
                !await client.DirectoryExists(remoteDirectory))
            {
                await client.CreateDirectory(remoteDirectory);
            }

            var destinationPath = Path.Combine(remoteDirectory, fileName);
            await using FileStream fs = new(localFilePath, FileMode.Open);
            await client.UploadStream(fs, destinationPath);
        });
    }

    public async Task<SubmitResult> DownloadFileAsync(int sftpconfigurationId, string remoteFilePath,
        string localFilePath)
    {
        return await ExecuteFtpOperationAsync(sftpconfigurationId,
            async client => { await client.DownloadFile(remoteFilePath, localFilePath); });
    }

    public async Task<SubmitResult> DeleteFileAsync(int sftpconfigurationId, string remoteFilePath)
    {
        return await ExecuteFtpOperationAsync(sftpconfigurationId,
            async client => { await client.DeleteFile(remoteFilePath); });
    }

    public async Task<SubmitResult> TestDirectoryAsync(int sftpconfigurationId, string? remoteFilePath,
        bool tryCreateFolder, CancellationToken _cts)
    {
        using var client = await GetClientAsync(sftpconfigurationId);
        bool checkAccess;

        try
        {
            await client.Connect(_cts);
            checkAccess = await client.DirectoryExists(remoteFilePath, _cts);
            if (!checkAccess && tryCreateFolder)
            {
                await client.CreateDirectory(remoteFilePath, _cts);
                checkAccess = await client.DirectoryExists(remoteFilePath, _cts);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Failed in testing directory [{remoteFilePath}]");
            return new SubmitResult { Success = false, Message = exception.Message };
        }
        finally
        {
            await client.Disconnect(_cts);
        }

        return new SubmitResult
        {
            Success = checkAccess,
            Message = checkAccess ? "Ok" : "Cannot reach the path"
        };
    }
}