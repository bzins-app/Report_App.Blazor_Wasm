﻿using System.Globalization;
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
    public async Task<IEnumerable<ApplicationLogTaskDetails>> GetLogTaskDetailsAsync(int logTaskHeaderId)
    {
        return await _context.ApplicationLogTaskDetails
            .AsNoTracking()
            .Where(a => a.TaskId == logTaskHeaderId)
            .ToArrayAsync();
    }

    [HttpGet]
    public async Task<IEnumerable<string>> GetTagsTasksAsync(TaskType type, int activityId = 0)
    {
        var query = _context.TaskHeader
            .AsNoTracking()
            .Where(a => a.Tags != "[]" && !string.IsNullOrEmpty(a.Tags) && a.Type == type);

        if (activityId > 0)
        {
            query = query.Where(a => a.IdActivity == activityId);
        }

        var _serializedTags = await query.Select(a => a.Tags).ToListAsync();
        var _tags = _serializedTags
            .SelectMany(value => JsonSerializer.Deserialize<List<string>>(value)!)
            .Distinct();

        return _tags;
    }

    [HttpGet]
    public async Task<IEnumerable<string>> GetTagsQueriesAsync(int activityId = 0)
    {
        var query = _context.QueryStore
            .AsNoTracking()
            .Where(a => a.Tags != "[]" && !string.IsNullOrEmpty(a.Tags));

        if (activityId > 0)
        {
            query = query.Where(a => a.IdActivity == activityId);
        }

        var _serializedTags = await query.Select(a => a.Tags).ToListAsync();
        var _tags = _serializedTags
            .SelectMany(value => JsonSerializer.Deserialize<List<string>>(value)!)
            .Distinct();

        return _tags;
    }

    [HttpGet]
    public async Task<IEnumerable<ActivityDbConnection>> GetActivityDbConnectionAsync(int activityId)
    {
        return await _context.ActivityDbConnection
            .AsNoTracking()
            .Where(a => a.Activity!.ActivityId == activityId)
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
    public async Task<IEnumerable<TaskEmailRecipient?>> GetTaskEmailRecipientAsync(int taskHeaderId)
    {
        return await _context.TaskEmailRecipient
            .AsNoTracking()
            .Where(a => a.TaskHeader.TaskHeaderId == taskHeaderId)
            .ToListAsync();
    }

    [HttpGet]
    public async Task<QueryStore?> GetQueryStoreAsync(int queryId)
    {
        return await _context.QueryStore
            .Include(a => a.Activity)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == queryId);
    }

    [HttpGet]
    public async Task<List<QueryStore>?> GetQueryStoreByActivityAsync(int activityId)
    {
        return await _context.QueryStore
            .AsNoTracking()
            .Where(a => a.IdActivity == activityId)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<IActionResult> DuplicateQueryStore(ApiCrudPayload<DuplicateQueryStore> values)
    {
        var queryToDuplicate = await _context.QueryStore
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == values.EntityValue.QueryId);

        if (queryToDuplicate == null)
        {
            return Ok(new SubmitResult { Success = false, Message = "Object not found" });
        }

        var activityInfo = await _context.Activity
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.ActivityId == values.EntityValue.ActivityId);

        var _newQuery = new QueryStore
        {
            QueryName = values.EntityValue.Name,
            Query = queryToDuplicate.Query,
            Tags = queryToDuplicate.Tags,
            QueryParameters = queryToDuplicate.QueryParameters,
            Parameters = queryToDuplicate.Parameters,
            IdActivity = activityInfo.ActivityId,
            Activity = activityInfo,
            ActivityName = activityInfo.ActivityName
        };

        return Ok(await InsertEntity(_newQuery, values.UserName!));
    }

    [HttpGet]
    public async Task<Activity> GetDataTransferInfoAsync()
    {
        var targetInfo = await _context.Activity
            .Include(a => a.ActivityDbConnections)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.ActivityType == ActivityType.TargetDb);

        if (targetInfo != null) return targetInfo;

        var connections = new List<ActivityDbConnection>
        {
            new ActivityDbConnection { Activity = targetInfo, TypeDb = TypeDb.SqlServer }
        };

        targetInfo = new Activity
        {
            ActivityName = "Data transfer",
            ActivityType = ActivityType.TargetDb,
            ActivityDbConnections = connections
        };

        return targetInfo;
    }

    [HttpGet]
    public async Task<ServicesStatus> GetServiceStatusAsync()
    {
        return (await _context.ServicesStatus
            .AsNoTracking()
            .OrderBy(a => a.Id)
            .FirstOrDefaultAsync())!;
    }

    [HttpPost]
    public async Task<IActionResult> ApplicationParametersUpdateAsync(ApiCrudPayload<ApplicationParameters> values)
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
    public async Task<IActionResult> ActivityInsert(ApiCrudPayload<Activity> values)
    {
        try
        {
            if (values.EntityValue.ActivityType == ActivityType.SourceDb)
            {
                if (!await _roleManager.RoleExistsAsync(values.EntityValue?.ActivityName!))
                {
                    await _roleManager.CreateAsync(new IdentityRole<Guid>(values.EntityValue.ActivityName!));
                    var users = await _userManager.GetUsersInRoleAsync("Admin");

                    foreach (var user in users)
                        if (!await _userManager.IsInRoleAsync(user, values.EntityValue.ActivityName!))
                            await _userManager.AddToRoleAsync(user, values.EntityValue.ActivityName!);
                }

                if (string.IsNullOrEmpty(values.EntityValue?.ActivityRoleId))
                {
                    var newRole = await _roleManager.FindByNameAsync(values.EntityValue?.ActivityName!);
                    values.EntityValue!.ActivityRoleId = newRole!.Id.ToString();
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
    public async Task<IActionResult> ActivityDelete(ApiCrudPayload<Activity> values)
    {
        try
        {
            var role = await _roleManager.FindByNameAsync(values.EntityValue.ActivityName!);

            if (role != null) await _roleManager.DeleteAsync(role);
            return Ok(await DeleteEntity(values.EntityValue, values.UserName!));
        }
        catch (Exception ex)
        {
            return Ok(new SubmitResult { Success = false, Message = ex.Message });
        }
    }


    [HttpPost]
    public async Task<IActionResult> ActivityUpdate(ApiCrudPayload<Activity> values)
    {
        try
        {
            if (values.EntityValue.ActivityType == ActivityType.SourceDb)
            {
                var roleActivity = await _roleManager.FindByIdAsync(values.EntityValue?.ActivityRoleId!);
                if (roleActivity != null && roleActivity.Name != values.EntityValue!.ActivityName)
                {
                    roleActivity.Name = values.EntityValue.ActivityName;
                    await _roleManager.UpdateAsync(roleActivity);
                }
            }

            if (values.EntityValue.ActivityDbConnections != null)
            {
                foreach (var connect in values.EntityValue.ActivityDbConnections)
                {
                    await UpdateEntity(connect, values.UserName!);
                }
            }

            return Ok(await UpdateEntity(values.EntityValue, values.UserName!));
        }
        catch (Exception ex)
        {
            return Ok(new SubmitResult { Success = false, Message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<TaskHeader> GetTaskHeaderAsync(int taskHeaderId)
    {
        return (await _context.TaskHeader
            .Include(a => a.TaskDetails)
            .Include(a => a.TaskEmailRecipients)
            .Include(a => a.Activity)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.TaskHeaderId == taskHeaderId))!;
    }

    public async Task<IEnumerable<EmailRecipient>> GetEmailsPerActivityAsync(int activityId)
    {
        var listTask = await _context.TaskEmailRecipient
            .Where(a => a.TaskHeader.Activity.ActivityId == activityId && a.Email != "[]")
            .Select(a => a.Email)
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
        return await _context.TaskHeader
            .Where(a => a.TaskHeaderId == taskHeaderId)
            .Include(a => a.TaskDetails)
            .AnyAsync();
    }

    [HttpPost]
    public async Task<IActionResult> TaskHeaderInsert(ApiCrudPayload<TaskHeader> values)
    {
        var result = await InsertEntity(values.EntityValue, values.UserName!);
        result.KeyValue = values.EntityValue.TaskHeaderId;
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> TaskActivate(ApiCrudPayload<TaskActivatePayload> values)
    {
        var entity = await _context.TaskHeader
            .FirstOrDefaultAsync(a => a.TaskHeaderId == values.EntityValue.TaskHeaderId);

        if (entity != null)
        {
            entity.IsActivated = values.EntityValue.Activate;
            var result = await UpdateEntity(entity, values.UserName!);
            return Ok(result);
        }

        return Ok(new SubmitResult { Success = false, Message = "TaskHeader not found" });
    }

    [HttpPost]
    public async Task<IActionResult> TaskSendByEmail(ApiCrudPayload<TaskActivatePayload> values)
    {
        var entity = await _context.TaskHeader
            .FirstOrDefaultAsync(a => a.TaskHeaderId == values.EntityValue.TaskHeaderId);

        if (entity != null)
        {
            entity.SendByEmail = values.EntityValue.Activate;
            var result = await UpdateEntity(entity, values.UserName!);
            return Ok(result);
        }

        return Ok(new SubmitResult { Success = false, Message = "TaskHeader not found" });
    }

    [HttpPost]
    public async Task<IActionResult> TaskHeaderDelete(ApiCrudPayload<TaskHeader> values)
    {
        return Ok(await DeleteEntity(values.EntityValue, values.UserName!));
    }

    [HttpPost]
    public async Task<IActionResult> TaskClone(ApiCrudPayload<DuplicateTask> values)
    {
        try
        {
            var dbItem = await _context.TaskHeader.Include(a => a.Activity).Include(a => a.TaskDetails)
                .Include(a => a.TaskEmailRecipients).Where(a => a.TaskHeaderId == values.EntityValue.TaskHeaderId)
                .AsNoTracking().FirstOrDefaultAsync();

            if (dbItem == null) return NotFound(new SubmitResult { Success = false, Message = "Item not found" });
            dbItem.TaskName = values.EntityValue!.Name;
            dbItem.IsActivated = false;
            dbItem.SendByEmail = dbItem.Type == TaskType.Alert;
            dbItem.FileDepositPathConfigurationId = 0;
            dbItem.TaskHeaderId = 0;

            if (dbItem.TaskDetails != null)
                foreach (var t in dbItem.TaskDetails)
                    t.TaskDetailId = 0;
            if (dbItem.TaskEmailRecipients != null)
                foreach (var t in dbItem.TaskEmailRecipients)
                    t.TaskEmailRecipientId = 0;
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
    public async Task<IActionResult> TaskHeaderUpdate(ApiCrudPayload<TaskHeader> values)
    {
        try
        {
            values.EntityValue.Activity = (await _context.Activity
                .Where(a => a.ActivityId == values.EntityValue.IdActivity).FirstOrDefaultAsync())!;

            _context.Entry(values.EntityValue).State = EntityState.Modified;
            _context.UpdateRange(values.EntityValue.TaskDetails);
            _context.UpdateRange(values.EntityValue.TaskEmailRecipients);
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
    public async Task<IActionResult> TaskDetailDelete(ApiCrudPayload<TaskDetail> values)
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
    public async Task<IActionResult> DepositPathInsert(ApiCrudPayload<FileDepositPathConfigurationDto> values)
    {
        var val = new FileDepositPathConfiguration();
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
    public async Task<IActionResult> DepositPathDelete(ApiCrudPayload<FileDepositPathConfigurationDto> values)
    {
        var val = await _context.FileDepositPathConfiguration
            .Where(a => a.FileDepositPathConfigurationId == values.EntityValue.FileDepositPathConfigurationId)
            .FirstOrDefaultAsync();
        return Ok(await DeleteEntity(val, values.UserName!));
    }

    [HttpPost]
    public async Task<IActionResult> DepositPathUpdate(ApiCrudPayload<FileDepositPathConfigurationDto> values)
    {
        var val = await _context.FileDepositPathConfiguration
            .Where(a => a.FileDepositPathConfigurationId == values.EntityValue.FileDepositPathConfigurationId)
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
    public async Task<IActionResult> QueryStoreInsert(ApiCrudPayload<QueryStore> values)
    {
        var result = await InsertEntity(values.EntityValue, values.UserName!);
        result.KeyValue = values.EntityValue.Id;
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> QueryStoreDelete(ApiCrudPayload<QueryStore> values)
    {
        return Ok(await DeleteEntity(values.EntityValue, values.UserName!));
    }

    [HttpPost]
    public async Task<IActionResult> UserConfigurationSave(ApiCrudPayload<UserConfigurationSave> item)
    {
        var user = await _userManager.FindByNameAsync(item.UserName!);
        if (user != null)
        {
            var config = new UserSavedConfiguration
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
            var config = await _context.UserSavedConfiguration
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
            return await _context.UserSavedConfiguration
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
    public async Task<IActionResult> QueryStoreUpdate(ApiCrudPayload<QueryStore> values)
    {
        values.EntityValue.Activity = (await _context.Activity.Where(a => a.ActivityId == values.EntityValue.IdActivity)
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

            var _descriptions = new List<DbTableDescriptions>();
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

            var _dbConnect = await _context.ActivityDbConnection
                .FirstOrDefaultAsync(a => a.Id == value.EntityValue.ActivityDbConnectionId);

            if (_dbConnect == null)
            {
                return Ok(new SubmitResult { Success = false, Message = "Database connection not found" });
            }

            var existingDescriptions = await _context.DbTableDescriptions
                .Where(a => a.ActivityDbConnection.Id == value.EntityValue.ActivityDbConnectionId)
                .ToListAsync();

            if (existingDescriptions.Any())
            {
                _context.DbTableDescriptions.RemoveRange(existingDescriptions);
                await SaveDbAsync(value.UserName);
            }

            foreach (var data in records)
            {
                if (!_descriptions.Any(a => a.TableName == data.TableName && a.ColumnName == data.ColumnName))
                {
                    _descriptions.Add(new DbTableDescriptions
                    {
                        TableName = data.TableName,
                        ColumnName = data.ColumnName,
                        TableDescription = data.TableDescription,
                        ColumnDescription = data.ColumnDescription,
                        IsSnippet = data.IsSnippet
                    });
                }
            }

            _dbConnect.DbTableDescriptions = _descriptions;
            _dbConnect.UseTablesDescriptions = true;
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