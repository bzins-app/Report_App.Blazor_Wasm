using FluentFTP;

namespace Report_App_WASM.Server.Services.FilesManagement;

public class FtpService : IDisposable
{
    // private readonly ILogger<FtpService> _logger;
    private readonly ApplicationDbContext _context;

    public FtpService( /*ILogger<FtpService> logger, */ ApplicationDbContext context)
    {
        //  _logger = logger;
        _context = context;
    }


    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private async Task<FileStorageConfiguration?> GetSftpConfigurationAsync(long sftpconfigurationId)
    {
        return await _context.FileStorageConfiguration.Where(a => a.FileStorageConfigurationId == sftpconfigurationId)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<FtpListItem>?> ListAllFilesAsync(int sftpconfigurationId,
        string remoteDirectory = ".")
    {
        using var client = await getClient( sftpconfigurationId);

        try
        {
            await client.AutoConnect();
            return await client.GetListing(remoteDirectory);
        }
        catch (Exception)
        {
            // _logger.LogError(exception, $"Failed in listing files under [{remoteDirectory}]");
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
        using var client = await getClient( sftpconfigurationId);

        try
        {
            await client.AutoConnect();
            if (tryCreateFolder && !await client.DirectoryExists(remoteDirectory))
                await client.CreateDirectory(remoteDirectory);
            var destinationPath = Path.Combine(remoteDirectory, fileName);
            await using FileStream fs = new(localFilePath, FileMode.Open);
            await client.UploadStream(fs, destinationPath);
            // _logger.LogInformation($"Finished uploading file [{localFilePath}] to [{remoteDirectory}]");
        }
        catch (Exception exception)
        {
            // _logger.LogError(exception, $"Failed in uploading file [{localFilePath}] to [{remoteDirectory}]");
            return new SubmitResult { Success = false, Message = exception.Message };
        }
        finally
        {
            await client.Disconnect();
        }

        return new SubmitResult { Success = true, Message = "Ok" };
    }


    private async Task<AsyncFtpClient> getClient(long sftpconfigurationId)
    {
        var config = await GetSftpConfigurationAsync(sftpconfigurationId);

        var client =string.IsNullOrEmpty(config.UserName)?new AsyncFtpClient(config.Host): new AsyncFtpClient(config.Host, config.UserName, EncryptDecrypt.DecryptString(config.Password));
        if (config.ConfigurationType == FileStorageConfigurationType.FTPs)
        {
            client.Config.EncryptionMode = FtpEncryptionMode.Auto;
            client.Config.ValidateAnyCertificate = true;
        }

        return client;
    }

    public async Task<SubmitResult> DownloadFileAsync(int sftpconfigurationId, string remoteFilePath,
        string localFilePath)
    {
        using var client = await getClient( sftpconfigurationId);

        try
        {
            await client.AutoConnect();
            await client.DownloadFile(remoteFilePath, localFilePath);
            //  _logger.LogInformation($"Finished downloading file [{localFilePath}] from [{remoteFilePath}]");
        }
        catch (Exception exception)
        {
            // _logger.LogError(exception, $"Failed in downloading file [{localFilePath}] from [{remoteFilePath}]");
            return new SubmitResult { Success = false, Message = exception.Message };
        }
        finally
        {
            await client.Disconnect();
        }

        return new SubmitResult { Success = true, Message = "Ok" };
    }

    public async Task<SubmitResult> DeleteFileAsync(int sftpconfigurationId, string remoteFilePath)
    {
        using var client = await getClient( sftpconfigurationId);

        try
        {
            await client.AutoConnect();
            await client.DeleteFile(remoteFilePath);
            // _logger.LogInformation($"File [{remoteFilePath}] deleted.");
        }
        catch (Exception exception)
        {
            //  _logger.LogError(exception, $"Failed in deleting file [{remoteFilePath}]");
            return new SubmitResult { Success = false, Message = exception.Message };
        }
        finally
        {
            await client.Disconnect();
        }

        return new SubmitResult { Success = true, Message = "Ok" };
    }

    public async Task<SubmitResult> TestDirectoryAsync(int sftpconfigurationId, string? remoteFilePath,
        bool tryCreateFolder , CancellationToken _cts)
    {
        using var client = await getClient( sftpconfigurationId);

        bool checkAcces;
        try
        {
            await client.AutoConnect(_cts);
            checkAcces = await client.DirectoryExists(remoteFilePath, _cts);
            if (!checkAcces && tryCreateFolder)
            {
                await client.CreateDirectory(remoteFilePath, _cts);
                checkAcces = await client.DirectoryExists(remoteFilePath, _cts);
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
            await client.Disconnect(_cts);
        }

        return new SubmitResult
        { Success = checkAcces, Message = checkAcces == false ? "Cannot reach the path" : "Ok" };
    }
}