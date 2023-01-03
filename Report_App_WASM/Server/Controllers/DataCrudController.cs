using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Utils;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.ApiExchanges;
using System.Globalization;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;

namespace Report_App_WASM.Server.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]

    public class DataCrudController : ControllerBase, IDisposable
    {

        private readonly ILogger<DataCrudController> _logger;
        private readonly ApplicationDbContext _context;
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
            return await _context.ApplicationLogTaskDetails.Where(a => a.TaskId == logTaskHeaderId).ToArrayAsync();
        }

        [HttpGet]
        public async Task<IEnumerable<ActivityDbConnection>> GetActivityDbConnectionAsync(int activityId)
        {
            return await _context.ActivityDbConnection.Where(a => a.Activity!.ActivityId == activityId).ToArrayAsync();
        }

        [HttpGet]
        public async Task<SftpConfiguration?> GetStfpConfigurationAsync(int sftpConfigurationId)
        {
            return await _context.SftpConfiguration.Where(a => a.SftpConfigurationId == sftpConfigurationId).FirstOrDefaultAsync();
        }

        [HttpGet]
        public async Task<IEnumerable<TaskEmailRecipient?>> GetTaskEmailRecipientAsync(int taskHeaderId)
        {
            return await _context.TaskEmailRecipient.Where(a => a.TaskHeader.TaskHeaderId == taskHeaderId).ToListAsync();
        }

        [HttpGet]
        public async Task<QueryStore?> GetQueryStoreAsync(int queryId)
        {
            return await _context.QueryStore.Include(a => a.Activity).Where(a => a.Id == queryId).FirstOrDefaultAsync();
        }

        [HttpGet]
        public async Task<Activity> GetDataTransferInfoAsync()
        {
            var targetInfo = await _context.Activity.Where(a => a.ActivityType == ActivityType.TargetDb).Include(a => a.ActivityDbConnections).FirstOrDefaultAsync();
            if (targetInfo != null) return targetInfo;
            List<ActivityDbConnection> connections = new();
            targetInfo = new Activity { ActivityName = "Data transfer", ActivityType = ActivityType.TargetDb };
            connections.Add(new ActivityDbConnection { Activity = targetInfo, TypeDb = TypeDb.SqlServer });
            targetInfo.ActivityDbConnections = connections;
            return targetInfo;
        }

        [HttpGet]
        public async Task<ServicesStatus> GetServiceStatusAsync()
        {
            return (await _context.ServicesStatus.OrderBy(a => a.Id).FirstOrDefaultAsync())!;
        }

        [HttpPost]
        public async Task<IActionResult> ApplicationParametersUpdateAsync(ApiCrudPayload<ApplicationParameters> values)
        {
            try
            {
                _context.Entry(entity: values.EntityValue!).State = EntityState.Modified;
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
                    var others = await _context.SmtpConfiguration.Where(a => a.Id != updateValues.Id).OrderBy(a => a.Id).ToListAsync();
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
                    var others = await _context.LdapConfiguration.Where(a => a.Id != updateValues.Id).OrderBy(a => a.Id).ToListAsync();
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
                if (values.EntityValue!.IsActivated)
                {
                    ApplicationConstants.LdapLogin = false;
                }
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
                if (!await _roleManager.RoleExistsAsync(values.EntityValue?.ActivityName!))
                {

                    await _roleManager.CreateAsync(new IdentityRole<Guid>(values.EntityValue.ActivityName!));
                    var users = await _userManager.GetUsersInRoleAsync("Admin");

                    foreach (var user in users)
                    {
                        if (!await _userManager.IsInRoleAsync(user, values.EntityValue.ActivityName!))
                        {
                            await _userManager.AddToRoleAsync(user, values.EntityValue.ActivityName!);
                            // await _SignIn.RefreshSignInAsync(user);
                        }
                    }
                }
                if (string.IsNullOrEmpty(values.EntityValue?.ActivityRoleId))
                {
                    var newRole = await _roleManager.FindByNameAsync(values.EntityValue?.ActivityName!);
                    values.EntityValue!.ActivityRoleId = newRole!.Id.ToString();
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

                if (role != null)
                {
                    await _roleManager.DeleteAsync(role);
                }
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
                var roleActivity = await _roleManager.FindByIdAsync(values.EntityValue?.ActivityRoleId!);
                if (roleActivity != null)
                {
                    if (roleActivity.Name != values.EntityValue!.ActivityName)
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
            return (await _context.TaskHeader.Include(a => a.TaskDetails).Include(a => a.TaskEmailRecipients).Include(a => a.Activity).Where(a => a.TaskHeaderId == taskHeaderId).OrderBy(a => a).FirstOrDefaultAsync())!;
        }

        public async Task<IEnumerable<EmailRecipient>> GetEmailsPerActivityAsync(int activityId)
        {
            List<EmailRecipient> emails = new();
            var listTask = await _context.TaskEmailRecipient.Where(a => a.TaskHeader.Activity.ActivityId == activityId && a.Email != "[]").Select(a => a.Email).ToListAsync();
            if (listTask != null)
            {
                foreach (var taskEmails in listTask.Select(value => JsonSerializer.Deserialize<List<EmailRecipient>>(value)))
                {
                    var e = taskEmails!.Select(a => new EmailRecipient { Email = a.Email }).ToList();
                    foreach (var value in e.Where(value => emails.All(a => a.Email != value.Email)))
                    {
                        if (!emails.Select(a => a.Email.ToLower()).Contains(value.Email.ToLower()))
                            emails.Add(value);
                    }
                }
            }

            return emails;
        }

        [HttpGet]
        public async Task<bool> GetTaskHasDetailsAsync(int taskHeaderId)
        {
            return await _context.TaskHeader.Where(a => a.TaskHeaderId == taskHeaderId).Include(a => a.TaskDetails).OrderBy(a => a).Select(a => a.TaskDetails).AnyAsync();
        }

        [HttpPost]
        public async Task<IActionResult> TaskHeaderInsert(ApiCrudPayload<TaskHeader> values)
        {
            var result = await InsertEntity(values.EntityValue, values.UserName!);
            result.KeyValue = values.EntityValue.TaskHeaderId;
            return Ok(result);
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

                var dbItem = await _context.TaskHeader.Include(a => a.Activity).Include(a => a.TaskDetails).Include(a => a.TaskEmailRecipients).Where(a => a.TaskHeaderId == values.EntityValue.TaskHeaderId).AsNoTracking().FirstOrDefaultAsync();

                if (dbItem == null) return NotFound(new SubmitResult { Success = false, Message = "Item not found" });
                dbItem.TaskName = values.EntityValue!.Name;
                dbItem.IsActivated = false;
                dbItem.SendByEmail = false;
                dbItem.FileDepositPathConfigurationId = 0;
                dbItem.TaskHeaderId = 0;

                if (dbItem.TaskDetails != null)
                {
                    foreach (var t in dbItem.TaskDetails)
                    {
                        t.TaskDetailId = 0;
                    }
                }
                if (dbItem.TaskEmailRecipients != null)
                {
                    foreach (var t in dbItem.TaskEmailRecipients)
                    {
                        t.TaskEmailRecipientId = 0;
                    }
                }
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

                values.EntityValue.Activity = (await _context.Activity.Where(a => a.ActivityId == values.EntityValue.IdActivity).FirstOrDefaultAsync())!;

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
        public async Task<IActionResult> DepositPathInsert(ApiCrudPayload<FileDepositPathConfiguration> values)
        {
            return Ok(await InsertEntity(values.EntityValue, values.UserName!));
        }

        [HttpPost]
        public async Task<IActionResult> DepositPathDelete(ApiCrudPayload<FileDepositPathConfiguration> values)
        {
            return Ok(await DeleteEntity(values.EntityValue, values.UserName!));
        }

        [HttpPost]
        public async Task<IActionResult> DepositPathUpdate(ApiCrudPayload<FileDepositPathConfiguration> values)
        {
            return Ok(await UpdateEntity(values.EntityValue, values.UserName!));
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
        public async Task<IActionResult> QueryStoreUpdate(ApiCrudPayload<QueryStore> values)
        {
            values.EntityValue.Activity = (await _context.Activity.Where(a => a.ActivityId == values.EntityValue.IdActivity).FirstOrDefaultAsync())!;
            return Ok(await UpdateEntity(values.EntityValue, values.UserName!));
        }
        public async Task<IActionResult> ImportTablesDescriptions(ApiCrudPayload<TablesDescriptionsImportPayload> value, CancellationToken ct)
        {
            try
            {
                if (value.EntityValue.FilePath != null)
                {
                    List<DbTableDescriptions> _descriptions = new List<DbTableDescriptions>();
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter = ";"
                    };

                    var records=new List<TableDescriptionCSV>();
                    using (var reader = new StreamReader($"wwwroot/{value.EntityValue.FilePath}"))
                    using (var csv = new CsvReader(reader, config))
                    {
                         records = csv.GetRecords<TableDescriptionCSV>().ToList();

                    }

                    var _dbConnect = await _context.ActivityDbConnection.Where(a => a.Id == value.EntityValue.ActivityDbConnectionId).FirstOrDefaultAsync(cancellationToken: ct);
                    if (await _context.DbTableDescriptions.Where(a =>
                            a.ActivityDbConnection.Id == value.EntityValue.ActivityDbConnectionId).AnyAsync(cancellationToken: ct))
                    {
                        var query = _context.DbTableDescriptions.Where(a =>
                            a.ActivityDbConnection.Id == value.EntityValue.ActivityDbConnectionId);
                        _context.RemoveRange(await query.ToListAsync(cancellationToken: ct));
                        await SaveDbAsync(value.UserName);
                    }
                    foreach (var data in records)
                    {
                        if (!_descriptions
                                .Any(a => a.TableName == data.TableName && a.ColumnName == data.ColumnName))
                        {
                            _descriptions.Add(new DbTableDescriptions
                            {
                                TableName = data.TableName,
                                ColumnName = data.ColumnName,
                                //ActivityDbConnection = _dbConnect,
                                TableDescription = data.TableDescription,
                                ColumnDescription = data.ColumnDescription
                            });
                        }
                    }


                    _dbConnect.DbTableDescriptions = _descriptions;

                    _dbConnect.UseTablesDescriptions = true;
                    _context.Entry(_dbConnect).State = EntityState.Modified;
                    await SaveDbAsync(value.UserName);
                    return Ok(new SubmitResult { Success = true });
                }
                else
                {
                    return Ok(new SubmitResult { Success = false, Message = "No file path" });
                }

            }
            catch (Exception e)
            {
                return Ok(new SubmitResult { Success = false, Message = e.Message });
            }


            return Ok();
        }

        private class TableDescriptionCSV
        {
            public string TableName { get; set; } = string.Empty;
            public string TableDescription { get; set; } = string.Empty;
            public string ColumnName { get; set; } = string.Empty;
            public string ColumnDescription { get; set; } = string.Empty;
        }

        private async Task<SubmitResult> InsertEntity<T>(T EntityValue, string UserName)
        {
            try
            {
                _context.Entry(entity: EntityValue!).State = EntityState.Added;
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
                _context.Entry(entity: EntityValue!).State = EntityState.Deleted;
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
                _context.Entry(entity: EntityValue!).State = EntityState.Modified;
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
    }
}
