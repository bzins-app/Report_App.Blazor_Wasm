using System.Globalization;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using Report_App_WASM.Shared.DTO;

namespace Report_App_WASM.Server.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
[ApiController]
[Route("api/[controller]/[action]")]
public class DataCrudController : ControllerBase, IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataCrudController> _logger;
    private readonly IMapper _mapper;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public DataCrudController(ILogger<DataCrudController> logger,
        ApplicationDbContext context, IMapper mapper,
        RoleManager<IdentityRole<Guid>> roleManager, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _mapper = mapper;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    [HttpGet]
    public async Task<IEnumerable<TaskStepLog>> GetLogTaskDetailsAsync(int logTaskHeaderId)
    {
        return await _context.TaskStepLog
            .AsNoTracking()
            .Where(a => a.TaskLogId == logTaskHeaderId)
            .ToArrayAsync();
    }

    [HttpGet]
    public async Task<IEnumerable<string>> GetTagsTasksAsync(TaskType type, int dataProviderId = 0)
    {
        var query = _context.ScheduledTask
            .AsNoTracking()
            .Where(a => a.Tags != "[]" && !string.IsNullOrEmpty(a.Tags) && a.Type == type);

        if (dataProviderId > 0)
        {
            query = query.Where(a => a.IdDataProvider == dataProviderId);
        }

        var _serializedTags = await query.Select(a => a.Tags).ToListAsync();
        var _tags = _serializedTags
            .SelectMany(value => JsonSerializer.Deserialize<List<string>>(value)!)
            .Distinct();

        return _tags;
    }

    [HttpGet]
    public async Task<IEnumerable<string>> GetTagsQueriesAsync(int dataProviderId = 0)
    {
        var query = _context.StoredQuery
            .AsNoTracking()
            .Where(a => a.Tags != "[]" && !string.IsNullOrEmpty(a.Tags));

        if (dataProviderId > 0)
        {
            query = query.Where(a => a.IdDataProvider == dataProviderId);
        }

        var _serializedTags = await query.Select(a => a.Tags).ToListAsync();
        var _tags = _serializedTags
            .SelectMany(value => JsonSerializer.Deserialize<List<string>>(value)!)
            .Distinct();

        return _tags;
    }

    [HttpGet]
    public async Task<IEnumerable<DatabaseConnection>> GetActivityDbConnectionAsync(int dataProviderId)
    {
        return await _context.DatabaseConnection
            .AsNoTracking()
            .Where(a => a.DataProvider!.DataProviderId == dataProviderId)
            .ToArrayAsync();
    }

    [HttpGet]
    public async Task<SftpConfiguration?> GetStfpConfigurationAsync(int sftpConfigurationId)
    {
        return await _context.SftpConfiguration
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.SftpConfigurationId == sftpConfigurationId);
    }

    [HttpGet]
    public async Task<IEnumerable<ScheduledTaskDistributionList?>> GetTaskEmailRecipientAsync(int taskHeaderId)
    {
        return await _context.ScheduledTaskDistributionList
            .AsNoTracking()
            .Where(a => a.ScheduledTask.ScheduledTaskId == taskHeaderId)
            .ToListAsync();
    }

    [HttpGet]
    public async Task<StoredQuery?> GetQueryStoreAsync(int queryId)
    {
        return await _context.StoredQuery
            .Include(a => a.DataProvider)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == queryId);
    }

    [HttpGet]
    public async Task<List<StoredQuery>?> GetQueryStoreByActivityAsync(int dataProviderId)
    {
        return await _context.StoredQuery
            .AsNoTracking()
            .Where(a => a.IdDataProvider == dataProviderId)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<IActionResult> DuplicateQueryStore(ApiCrudPayload<DuplicateQueryStore> values)
    {
        var queryToDuplicate = await _context.StoredQuery
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == values.EntityValue.QueryId);

        if (queryToDuplicate == null)
        {
            return Ok(new SubmitResult { Success = false, Message = "Object not found" });
        }

        var activityInfo = await _context.DataProvider
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.DataProviderId == values.EntityValue.DataProviderId);

        var _newQuery = new StoredQuery
        {
            QueryName = values.EntityValue.Name,
            Query = queryToDuplicate.Query,
            Tags = queryToDuplicate.Tags,
            QueryParameters = queryToDuplicate.QueryParameters,
            Parameters = queryToDuplicate.Parameters,
            IdDataProvider = activityInfo.DataProviderId,
            DataProvider = activityInfo,
            ProviderName = activityInfo.ProviderName
        };

        return Ok(await InsertEntity(_newQuery, values.UserName!));
    }

    [HttpGet]
    public async Task<DataProvider> GetDataTransferInfoAsync()
    {
        var targetInfo = await _context.DataProvider
            .Include(a => a.DatabaseConnection)
            .AsNoTracking()
            .FirstOrDefaultAsync((a) => a.ProviderType == ProviderType.TargetDatabase);

        if (targetInfo != null) return targetInfo;

        var connections = new List<DatabaseConnection>
            { new DatabaseConnection { DataProvider = targetInfo, TypeDb = TypeDb.SqlServer } };

        targetInfo = new DataProvider
        {
            ProviderName = "Data transfer",
            ProviderType = ProviderType.TargetDatabase,
            DatabaseConnection = connections
        };

        return targetInfo;
    }

    [HttpGet]
    public async Task<SystemServicesStatus> GetServiceStatusAsync()
    {
        return (await _context.SystemServicesStatus
            .AsNoTracking()
            .OrderBy(a => a.Id)
            .FirstOrDefaultAsync())!;
    }

    [HttpPost]
    public async Task<IActionResult> ApplicationParametersUpdateAsync(ApiCrudPayload<SystemParameters> values)
    {
        try
        {
            _context.Entry(values.EntityValue!).State = EntityState.Modified;
            await SaveDbAsync(values.UserName);
            ApplicationConstants.ApplicationName = values.EntityValue!.ApplicationName;
            ApplicationConstants.ApplicationLogo = values.EntityValue.ApplicationLogo;
            ApplicationConstants.ActivateAdHocQueriesModule = values.EntityValue!.ActivateAdHocQueriesModule;
            ApplicationConstants.ActivateTaskSchedulerModule = values.EntityValue.ActivateTaskSchedulerModule;
            return Ok(new SubmitResult { Success = true });
        }
        catch (Exception ex)
        {
            return Ok(new SubmitResult { Success = false, Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SmtpInsert(ApiCrudPayload<SmtpConfiguration> values)
    {
        return Ok(await InsertEntity(values.EntityValue, values.UserName!));
    }

    [HttpPost]
    public async Task<IActionResult> SmtpDelete(ApiCrudPayload<SmtpConfiguration> values)
    {
        return Ok(await DeleteEntity(values.EntityValue, values.UserName!));
    }

    [HttpPost]
    public async Task<IActionResult> SmtpUpdate(ApiCrudPayload<SmtpConfiguration> values)
    {
        return Ok(await UpdateEntity(values.EntityValue, values.UserName!));
    }

    [HttpPost]
    public async Task<IActionResult> SmtpActivate(ApiCrudPayload<SmtpConfiguration> values)
    {
        try
        {
            var updateValues = values.EntityValue;
            if (updateValues!.IsActivated)
            {
                var others = await _context.SmtpConfiguration
                    .Where(a => a.Id != updateValues.Id)
                    .ToListAsync();

                foreach (var item in others)
                {
                    item.IsActivated = false;
                    _context.Entry(item).State = EntityState.Modified;
                }
            }

            _context.Entry(updateValues).State = EntityState.Modified;
            await SaveDbAsync(values.UserName);
            return Ok(new SubmitResult { Success = true });
        }
        catch (Exception ex)
        {
            return Ok(new SubmitResult { Success = false, Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> LdapActivate(ApiCrudPayload<LdapConfiguration> values)
    {
        try
        {
            var updateValues = values.EntityValue;

            if (updateValues.IsActivated)
            {
                var others = await _context.LdapConfiguration
                    .Where(a => a.Id != updateValues.Id)
                    .ToListAsync();

                foreach (var item in others)
                {
                    item.IsActivated = false;
                    _context.Entry(item).State = EntityState.Modified;
                }
            }

            _context.Entry(updateValues).State = EntityState.Modified;
            await SaveDbAsync(values.UserName);
            return Ok(new SubmitResult { Success = true });
        }
        catch (Exception ex)
        {
            return Ok(new SubmitResult { Success = false, Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> LdapInsert(ApiCrudPayload<LdapConfiguration> values)
    {
        return Ok(await InsertEntity(values.EntityValue, values.UserName!));
    }

    [HttpPost]
    public async Task<IActionResult> LdapDelete(ApiCrudPayload<LdapConfiguration> values)
    {
        try
        {
            _context.Remove(values.EntityValue!);
            if (values.EntityValue!.IsActivated) ApplicationConstants.LdapLogin = false;
            await SaveDbAsync(values.UserName);

            return Ok(new SubmitResult { Success = true });
        }
        catch (Exception ex)
        {
            return Ok(new SubmitResult { Success = false, Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> LdapUpdate(ApiCrudPayload<LdapConfiguration> values)
    {
        try
        {
            _context.Update(values.EntityValue!);
            await SaveDbAsync(values.UserName);

            ApplicationConstants.LdapLogin = values.EntityValue.IsActivated;

            return Ok(new SubmitResult { Success = true });
        }
        catch (Exception ex)
        {
            return Ok(new SubmitResult { Success = false, Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ActivityInsert(ApiCrudPayload<DataProvider> values)
    {
        try
        {
            if (values.EntityValue.ProviderType == ProviderType.SourceDatabase)
            {
                if (!await _roleManager.RoleExistsAsync(values.EntityValue?.ProviderName!))
                {
                    await _roleManager.CreateAsync(new IdentityRole<Guid>(values.EntityValue.ProviderName!));
                    var users = await _userManager.GetUsersInRoleAsync("Admin");

                    foreach (var user in users)
                        if (!await _userManager.IsInRoleAsync(user, values.EntityValue.ProviderName!))
                            await _userManager.AddToRoleAsync(user, values.EntityValue.ProviderName!);
                    //await _signInManager.RefreshSignInAsync(user);

                    //demo
                    var usersDemo = await _userManager.GetUsersInRoleAsync("Demo");

                    foreach (var user in usersDemo)
                        if (!await _userManager.IsInRoleAsync(user, values.EntityValue.ProviderName!))
                            await _userManager.AddToRoleAsync(user, values.EntityValue.ProviderName!);
                }

                if (string.IsNullOrEmpty(values.EntityValue?.ProviderRoleId))
                {
                    var newRole = await _roleManager.FindByNameAsync(values.EntityValue?.ProviderName!);
                    values.EntityValue!.ProviderRoleId = newRole!.Id.ToString();
                }
            }

            await _context.AddAsync(values.EntityValue);
            await SaveDbAsync(values.UserName);
            _context.Entry(values.EntityValue).State = EntityState.Detached;
            return Ok(new SubmitResult { Success = true });
        }
        catch (Exception ex)
        {
            return Ok(new SubmitResult { Success = false, Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ActivityDelete(ApiCrudPayload<DataProvider> values)
    {
        try
        {
            var role = await _roleManager.FindByNameAsync(values.EntityValue.ProviderName!);

            if (role != null) await _roleManager.DeleteAsync(role);
            return Ok(await DeleteEntity(values.EntityValue, values.UserName!));
        }
        catch (Exception ex)
        {
            return Ok(new SubmitResult { Success = false, Message = ex.Message });
        }
    }


    [HttpPost]
    public async Task<IActionResult> ActivityUpdate(ApiCrudPayload<DataProvider> values)
    {
        try
        {
            if (values.EntityValue.ProviderType == ProviderType.SourceDatabase)
            {
                var roleActivity = await _roleManager.FindByIdAsync(values.EntityValue?.ProviderRoleId!);
                if (roleActivity != null && roleActivity.Name != values.EntityValue!.ProviderName)
                {
                    roleActivity.Name = values.EntityValue.ProviderName;
                    await _roleManager.UpdateAsync(roleActivity);
                }
            }

            if (values.EntityValue.DatabaseConnection != null)
            {
                await UpdateEntity(values.EntityValue.DatabaseConnection, values.UserName!);
            }

            return Ok(await UpdateEntity(values.EntityValue, values.UserName!));
        }
        catch (Exception ex)
        {
            return Ok(new SubmitResult { Success = false, Message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<ScheduledTask> GetTaskHeaderAsync(int taskHeaderId)
    {
        return (await _context.ScheduledTask
            .Include(a => a.TaskQueries)
            .Include(a => a.DistributionLists)
            .Include(a => a.DataProvider)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.ScheduledTaskId == taskHeaderId))!;
    }

    public async Task<IEnumerable<EmailRecipient>> GetEmailsPerActivityAsync(int dataProviderId)
    {
        var listTask = await _context.ScheduledTaskDistributionList
            .Where(a => a.ScheduledTask.DataProvider.DataProviderId == dataProviderId && a.Recipients != "[]")
            .Select(a => a.Recipients)
            .ToListAsync();

        var emails = listTask
            .SelectMany(value => JsonSerializer.Deserialize<List<EmailRecipient>>(value)!)
            .GroupBy(e => e.Email.ToLower())
            .Select(g => g.First())
            .ToList();

        return emails;
    }

    [HttpGet]
    public async Task<bool> GetTaskHasDetailsAsync(int taskHeaderId)
    {
        return await _context.ScheduledTask
            .Where(a => a.ScheduledTaskId == taskHeaderId)
            .Include(a => a.TaskQueries)
            .AnyAsync();
    }

    [HttpPost]
    public async Task<IActionResult> TaskHeaderInsert(ApiCrudPayload<ScheduledTask> values)
    {
        var result = await InsertEntity(values.EntityValue, values.UserName!);
        result.KeyValue = values.EntityValue.ScheduledTaskId;
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> TaskActivate(ApiCrudPayload<TaskActivatePayload> values)
    {
        var entity = await _context.ScheduledTask
            .FirstOrDefaultAsync(a => a.ScheduledTaskId == values.EntityValue.ScheduledTaskId);

        if (entity != null)
        {
            entity.IsEnabled = values.EntityValue.Activate;
            var result = await UpdateEntity(entity, values.UserName!);
            return Ok(result);
        }

        return Ok(new SubmitResult { Success = false, Message = "TaskHeader not found" });
    }

    [HttpPost]
    public async Task<IActionResult> TaskSendByEmail(ApiCrudPayload<TaskActivatePayload> values)
    {
        var entity = await _context.ScheduledTask
            .FirstOrDefaultAsync(a => a.ScheduledTaskId == values.EntityValue.ScheduledTaskId);

        if (entity != null)
        {
            entity.SendByEmail = values.EntityValue.Activate;
            var result = await UpdateEntity(entity, values.UserName!);
            return Ok(result);
        }

        return Ok(new SubmitResult { Success = false, Message = "TaskHeader not found" });
    }

    [HttpPost]
    public async Task<IActionResult> TaskHeaderDelete(ApiCrudPayload<ScheduledTask> values)
    {
        return Ok(await DeleteEntity(values.EntityValue, values.UserName!));
    }

    [HttpPost]
    public async Task<IActionResult> TaskClone(ApiCrudPayload<DuplicateTask> values)
    {
        try
        {
            var dbItem = await _context.ScheduledTask.Include(a => a.DataProvider).Include(a => a.TaskQueries)
                .Include(scheduledTask => scheduledTask.DistributionLists)
                .Include(a => a.ScheduledTaskId).Where(a => a.ScheduledTaskId == values.EntityValue.ScheduledTaskId)
                .AsNoTracking().FirstOrDefaultAsync();

            if (dbItem == null) return NotFound(new SubmitResult { Success = false, Message = "Item not found" });
            dbItem.TaskName = values.EntityValue!.Name;
            dbItem.IsEnabled = false;
            dbItem.SendByEmail = dbItem.Type == TaskType.Alert;
            dbItem.FileStorageLocationId = 0;
            dbItem.ScheduledTaskId = 0;

            if (dbItem.TaskQueries != null)
                foreach (var t in dbItem.TaskQueries)
                    t.ScheduledTaskQueryId = 0;
            if (dbItem.DistributionLists != null)
                foreach (var t in dbItem.DistributionLists)
                    t.ScheduledTaskDistributionListId = 0;
            _context.Update(dbItem);
            await SaveDbAsync(values.UserName);
            _context.Entry(dbItem).State = EntityState.Detached;
            _context.Entry(values.EntityValue).State = EntityState.Deleted;


            return Ok(new SubmitResult { Success = true });
        }
        catch (Exception ex)
        {
            return Ok(new SubmitResult { Success = true, Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> TaskHeaderUpdate(ApiCrudPayload<ScheduledTask> values)
    {
        try
        {
            values.EntityValue.DataProvider = (await _context.DataProvider
                .Where(a => a.DataProviderId == values.EntityValue.IdDataProvider).FirstOrDefaultAsync())!;

            _context.Entry(values.EntityValue).State = EntityState.Modified;
            _context.UpdateRange(values.EntityValue.TaskQueries);
            _context.UpdateRange(values.EntityValue.DistributionLists);
            await SaveDbAsync(values.UserName);
            _context.Entry(values.EntityValue).State = EntityState.Detached;
            return Ok(new SubmitResult { Success = true });
        }
        catch (Exception ex)
        {
            return Ok(new SubmitResult { Success = false, Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> TaskDetailDelete(ApiCrudPayload<ScheduledTaskQuery> values)
    {
        return Ok(await DeleteEntity(values.EntityValue, values.UserName!));
    }

    [HttpPost]
    public async Task<IActionResult> SftpInsert(ApiCrudPayload<SftpConfiguration> values)
    {
        return Ok(await InsertEntity(values.EntityValue, values.UserName!));
    }

    [HttpPost]
    public async Task<IActionResult> SftpDelete(ApiCrudPayload<SftpConfiguration> values)
    {
        return Ok(await DeleteEntity(values.EntityValue, values.UserName!));
    }

    [HttpPost]
    public async Task<IActionResult> SftpUpdate(ApiCrudPayload<SftpConfiguration> values)
    {
        return Ok(await UpdateEntity(values.EntityValue, values.UserName!));
    }

    [HttpPost]
    public async Task<IActionResult> DepositPathInsert(ApiCrudPayload<FileStorageLocationDto> values)
    {
        var val = new FileStorageLocation();
        if (values.EntityValue.SftpConfigurationId > 0)
            val.SftpConfiguration = await _context.SftpConfiguration
                .Where(a => a.SftpConfigurationId == values.EntityValue.SftpConfigurationId).FirstOrDefaultAsync();

        val.ConfigurationName = values.EntityValue.ConfigurationName;
        val.FilePath = values.EntityValue.FilePath;
        val.TryToCreateFolder = values.EntityValue.TryToCreateFolder;
        val.UseSftpProtocol = values.EntityValue.UseSftpProtocol;

        return Ok(await InsertEntity(val, values.UserName!));
    }

    [HttpPost]
    public async Task<IActionResult> DepositPathDelete(ApiCrudPayload<FileStorageLocationDto> values)
    {
        var val = await _context.FileStorageLocation
            .Where(a => a.FileStorageLocationId == values.EntityValue.FileStorageLocationId)
            .FirstOrDefaultAsync();
        return Ok(await DeleteEntity(val, values.UserName!));
    }

    [HttpPost]
    public async Task<IActionResult> DepositPathUpdate(ApiCrudPayload<FileStorageLocationDto> values)
    {
        var val = await _context.FileStorageLocation
            .Where(a => a.FileStorageLocationId == values.EntityValue.FileStorageLocationId)
            .FirstOrDefaultAsync();
        if (values.EntityValue.SftpConfigurationId > 0)
            val.SftpConfiguration = await _context.SftpConfiguration
                .Where(a => a.SftpConfigurationId == values.EntityValue.SftpConfigurationId).FirstOrDefaultAsync();

        val.ConfigurationName = values.EntityValue.ConfigurationName;
        val.FilePath = values.EntityValue.FilePath;
        val.TryToCreateFolder = values.EntityValue.TryToCreateFolder;
        val.UseSftpProtocol = values.EntityValue.UseSftpProtocol;

        return Ok(await UpdateEntity(val, values.UserName!));
    }

    [HttpPost]
    public async Task<IActionResult> QueryStoreInsert(ApiCrudPayload<StoredQuery> values)
    {
        var result = await InsertEntity(values.EntityValue, values.UserName!);
        result.KeyValue = values.EntityValue.Id;
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> QueryStoreDelete(ApiCrudPayload<StoredQuery> values)
    {
        return Ok(await DeleteEntity(values.EntityValue, values.UserName!));
    }

    [HttpPost]
    public async Task<IActionResult> UserConfigurationSave(ApiCrudPayload<UserConfigurationSave> item)
    {
        var user = await _userManager.FindByNameAsync(item.UserName!);
        if (user != null)
        {
            var config = new UserPreferences
            {
                SaveName = item.EntityValue.SaveName, IdIntConfiguration = item.EntityValue.IdIntConfiguration,
                TypeConfiguration = item.EntityValue.TypeConfiguration, SavedValues = item.EntityValue.SavedValues,
                UserId = user.Id.ToString()
            };
            return Ok(await InsertEntity(config, item.UserName!));
        }

        return Ok(new SubmitResult { Success = false, Message = "User not found" });
    }

    [HttpPost]
    public async Task<IActionResult> UserConfigurationDelete(ApiCrudPayload<UserConfigurationDelete> item)
    {
        var user = await _userManager.FindByNameAsync(item.UserName!);
        if (user != null)
        {
            var config = await _context.UserPreferences
                .Where(a => a.Id == item.EntityValue.Id && a.UserId == user.Id.ToString())
                .FirstOrDefaultAsync();
            if (config != null) return Ok(await DeleteEntity(config, item.UserName!));
            return Ok(new SubmitResult { Success = true, Message = "Configuration not found" });
        }

        return Ok(new SubmitResult { Success = false, Message = "User not found" });
    }

    [HttpGet]
    public async Task<List<UserConfigurations>> UserConfigurationGet(int IdIntConfiguration, string UserName)
    {
        var user = await _userManager.FindByNameAsync(UserName);

        if (user != null)
            return await _context.UserPreferences
                .Where(a => a.IdIntConfiguration == IdIntConfiguration && a.UserId == user.Id.ToString())
                .Select(a => new UserConfigurations
                {
                    SaveName = a.SaveName, Id = a.Id, IdIntConfiguration = a.IdIntConfiguration,
                    TypeConfiguration = a.TypeConfiguration, SavedValues = a.SavedValues, Parameters = a.Parameters,
                    IdStringConfiguration = a.IdStringConfiguration
                }).ToListAsync();

        return new List<UserConfigurations>();
    }

    [HttpPost]
    public async Task<IActionResult> QueryStoreUpdate(ApiCrudPayload<StoredQuery> values)
    {
        values.EntityValue.DataProvider = (await _context.DataProvider
            .Where(a => a.DataProviderId == values.EntityValue.IdDataProvider)
            .FirstOrDefaultAsync())!;
        return Ok(await UpdateEntity(values.EntityValue, values.UserName!));
    }

    public async Task<IActionResult> ImportTablesDescriptions(ApiCrudPayload<TablesDescriptionsImportPayload> value)
    {
        try
        {
            if (value.EntityValue.FilePath == null)
            {
                return Ok(new SubmitResult { Success = false, Message = "No file path" });
            }

            var _descriptions = new List<TableMetadata>();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";"
            };

            List<TableDescriptionCSV> records;
            using (var reader = new StreamReader($"wwwroot/{value.EntityValue.FilePath}", true))
            using (var csv = new CsvReader(reader, config))
            {
                records = csv.GetRecords<TableDescriptionCSV>().ToList();
            }

            var _dbConnect = await _context.DatabaseConnection
                .FirstOrDefaultAsync(a => a.DatabaseConnectionId == value.EntityValue.DatabaseConnectionId);

            if (_dbConnect == null)
            {
                return Ok(new SubmitResult { Success = false, Message = "Database connection not found" });
            }

            var existingDescriptions = await _context.TableMetadata
                .Where(a => a.DatabaseConnection.DatabaseConnectionId == value.EntityValue.DatabaseConnectionId)
                .ToListAsync();

            if (existingDescriptions.Any())
            {
                _context.TableMetadata.RemoveRange(existingDescriptions);
                await SaveDbAsync(value.UserName);
            }

            foreach (var data in records)
            {
                if (!_descriptions.Any(a => a.TableName == data.TableName && a.ColumnName == data.ColumnName))
                {
                    _descriptions.Add(new TableMetadata
                    {
                        TableName = data.TableName,
                        ColumnName = data.ColumnName,
                        TableDescription = data.TableDescription,
                        ColumnDescription = data.ColumnDescription,
                        IsSnippet = data.IsSnippet
                    });
                }
            }

            _dbConnect.TableMetadata = _descriptions;
            _dbConnect.UseTableMetaData = true;
            _context.Entry(_dbConnect).State = EntityState.Modified;
            await SaveDbAsync(value.UserName);

            return Ok(new SubmitResult { Success = true });
        }
        catch (Exception e)
        {
            return Ok(new SubmitResult { Success = false, Message = e.Message });
        }
    }

    private async Task<SubmitResult> InsertEntity<T>(T EntityValue, string UserName)
    {
        try
        {
            _context.Entry(EntityValue!).State = EntityState.Added;
            await SaveDbAsync(UserName);
            return new SubmitResult { Success = true };
        }
        catch (Exception ex)
        {
            return new SubmitResult { Success = false, Message = ex.Message };
        }
    }

    private async Task<SubmitResult> DeleteEntity<T>(T EntityValue, string UserName)
    {
        try
        {
            _context.Entry(EntityValue!).State = EntityState.Deleted;
            await SaveDbAsync(UserName);
            return new SubmitResult { Success = true };
        }
        catch (Exception ex)
        {
            return new SubmitResult { Success = false, Message = ex.Message };
        }
    }

    private async Task<SubmitResult> UpdateEntity<T>(T EntityValue, string UserName)
    {
        try
        {
            _context.Entry(EntityValue!).State = EntityState.Modified;
            await SaveDbAsync(UserName);
            return new SubmitResult { Success = true };
        }
        catch (Exception ex)
        {
            return new SubmitResult { Success = false, Message = ex.Message };
        }
    }

    private async Task SaveDbAsync(string? userId = "system")
    {
        await _context.SaveChangesAsync(userId);
    }

    private class TableDescriptionCSV
    {
        public string TableName { get; set; } = string.Empty;
        public string TableDescription { get; set; } = string.Empty;
        public string ColumnName { get; set; } = string.Empty;
        public string ColumnDescription { get; set; } = string.Empty;
        public bool IsSnippet { get; set; }
    }
}