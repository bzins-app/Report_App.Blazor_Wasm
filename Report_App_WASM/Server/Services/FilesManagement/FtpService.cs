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

    private async Task<SftpConfiguration?> GetSftpConfigurationAsync(int sftpconfigurationId)
    {
        return await _context.SftpConfiguration.Where(a => a.SftpConfigurationId == sftpconfigurationId).AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<FtpListItem>?> ListAllFilesAsync(int sftpconfigurationId,
        string remoteDirectory = ".")
    {
        var config = await GetSftpConfigurationAsync(sftpconfigurationId);

        using var client =
            new AsyncFtpClient(config.Host, config.UserName, EncryptDecrypt.DecryptString(config.Password));
        try
        {
            await client.Connect();
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

    public async Task<SubmitResult> UploadFileAsync(int sftpconfigurationId, string localFilePath,
        string remoteDirectory, string fileName, bool tryCreateFolder = false)
    {
        var config = await GetSftpConfigurationAsync(sftpconfigurationId);

        using var client =
            new AsyncFtpClient(config.Host, config.UserName, EncryptDecrypt.DecryptString(config.Password));

        try
        {
            await client.Connect();
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

    public async Task<SubmitResult> DownloadFileAsync(int sftpconfigurationId, string remoteFilePath,
        string localFilePath)
    {
        var config = await GetSftpConfigurationAsync(sftpconfigurationId);

        using var client =
            new AsyncFtpClient(config.Host, config.UserName, EncryptDecrypt.DecryptString(config.Password));

        try
        {
            await client.Connect();
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
        var config = await GetSftpConfigurationAsync(sftpconfigurationId);

        using var client =
            new AsyncFtpClient(config.Host, config.UserName, EncryptDecrypt.DecryptString(config.Password));

        try
        {
            await client.Connect();
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
        bool tryCreateFolder = false)
    {
        var config = await GetSftpConfigurationAsync(sftpconfigurationId);

        using var client =
            new AsyncFtpClient(config.Host, config.UserName, EncryptDecrypt.DecryptString(config.Password));

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
            return new SubmitResult { Success = false, Message = exception.Message };
        }
        finally
        {
            await client.Disconnect();
        }

        return new SubmitResult
            { Success = checkAcces, Message = checkAcces == false ? "Cannot reach the path" : "Ok" };
    }
}