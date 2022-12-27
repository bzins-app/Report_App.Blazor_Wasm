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

namespace Report_App_WASM.Server.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]

    public class DataCrudController : ControllerBase
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
        public async Task<TaskEmailRecipient?> GetTaskEmailRecipientAsync(int taskHeaderId)
        {
            return await _context.TaskEmailRecipient.Where(a => a.TaskHeader.TaskHeaderId == taskHeaderId).FirstOrDefaultAsync();
        }

        [HttpGet]
        public async Task<Activity> GetDataTransferInfoAsync()
        {
            var targetInfo = await _context.Activity.Where(a => a.ActivityType == ActivityType.TargetDb).Include(a => a.ActivityDbConnections).FirstOrDefaultAsync();
            if (targetInfo == null)
            {
                List<ActivityDbConnection> connections = new();
                targetInfo = new() { ActivityName = "Data transfer", ActivityType = ActivityType.TargetDb };
                connections.Add(new() { Activity = targetInfo, TypeDb = TypeDb.SqlServer });
                targetInfo.ActivityDbConnections = connections;
            }
            return targetInfo;
        }

        [HttpGet]
        public async Task<ServicesStatus> GetServiceStatusAsync()
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await _context.ServicesStatus.OrderBy(a => a.Id).FirstOrDefaultAsync();
#pragma warning restore CS8603 // Possible null reference return.
        }

        [HttpPost]
        public async Task<IActionResult> ApplicationParametersUpdateAsync(ApiCrudPayload<ApplicationParameters> values)
        {
            try
            {
#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.ApplicationParameters?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Update<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.ApplicationParameters?' doesn't match 'class' constraint.
                _context.Update(values.EntityValue);
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.ApplicationParameters?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Update<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.ApplicationParameters?' doesn't match 'class' constraint.
                await SaveDbAsync(values.UserName);
                ApplicationConstants.ApplicationName = values.EntityValue!.ApplicationName;
                ApplicationConstants.ApplicationLogo = values.EntityValue.ApplicationLogo;
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
            try
            {
#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.SmtpConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.AddAsync<TEntity>(TEntity, CancellationToken)'. Nullability of type argument 'Report_App_WASM.Server.Models.SmtpConfiguration?' doesn't match 'class' constraint.
                await _context.AddAsync(values.EntityValue);
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.SmtpConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.AddAsync<TEntity>(TEntity, CancellationToken)'. Nullability of type argument 'Report_App_WASM.Server.Models.SmtpConfiguration?' doesn't match 'class' constraint.
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SmtpDelete(ApiCrudPayload<SmtpConfiguration> values)
        {
            try
            {
#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.SmtpConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Remove<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.SmtpConfiguration?' doesn't match 'class' constraint.
                if (_context != null) _context.Remove(values.EntityValue);
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.SmtpConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Remove<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.SmtpConfiguration?' doesn't match 'class' constraint.
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SmtpUpdate(ApiCrudPayload<SmtpConfiguration> values)
        {
            try
            {
#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.SmtpConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Update<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.SmtpConfiguration?' doesn't match 'class' constraint.
                _context.Update(values.EntityValue);
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.SmtpConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Update<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.SmtpConfiguration?' doesn't match 'class' constraint.
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
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
            try
            {
#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.LdapConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.AddAsync<TEntity>(TEntity, CancellationToken)'. Nullability of type argument 'Report_App_WASM.Server.Models.LdapConfiguration?' doesn't match 'class' constraint.
                await _context.AddAsync(values.EntityValue);
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.LdapConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.AddAsync<TEntity>(TEntity, CancellationToken)'. Nullability of type argument 'Report_App_WASM.Server.Models.LdapConfiguration?' doesn't match 'class' constraint.
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> LdapDelete(ApiCrudPayload<LdapConfiguration> values)
        {
            try
            {
                _context.Remove(values.EntityValue);
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
                _context.Update(values.EntityValue);
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
#pragma warning disable CS8604 // Possible null reference argument for parameter 'roleName' in 'IdentityRole<Guid>.IdentityRole(string roleName)'.

                    await _roleManager.CreateAsync(new(values.EntityValue.ActivityName));

#pragma warning restore CS8604 // Possible null reference argument for parameter 'roleName' in 'IdentityRole<Guid>.IdentityRole(string roleName)'.
                    var users = await _userManager.GetUsersInRoleAsync("Admin");

                    foreach (var user in users)
                    {
                        if (!await _userManager.IsInRoleAsync(user, values.EntityValue.ActivityName))
                        {
                            await _userManager.AddToRoleAsync(user, values.EntityValue.ActivityName);
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
#pragma warning disable CS8604 // Possible null reference argument for parameter 'roleName' in 'Task<IdentityRole<Guid>?> RoleManager<IdentityRole<Guid>>.FindByNameAsync(string roleName)'.

                var role = await _roleManager.FindByNameAsync(values.EntityValue.ActivityName);

#pragma warning restore CS8604 // Possible null reference argument for parameter 'roleName' in 'Task<IdentityRole<Guid>?> RoleManager<IdentityRole<Guid>>.FindByNameAsync(string roleName)'.
                if (role != null)
                {
                    await _roleManager.DeleteAsync(role);
                }
                _context.Entry(values.EntityValue).State = EntityState.Deleted;
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
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

#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.Activity?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Update<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.Activity?' doesn't match 'class' constraint.
                _context.Update(values.EntityValue);
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.Activity?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Update<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.Activity?' doesn't match 'class' constraint.
                await SaveDbAsync(values.UserName);
#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.Activity?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Entry<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.Activity?' doesn't match 'class' constraint.
                _context.Entry(values.EntityValue).State = EntityState.Detached;
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.Activity?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Entry<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.Activity?' doesn't match 'class' constraint.

                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<TaskHeader> GetTaskHeaderAsync(int taskHeaderId)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await _context.TaskHeader.Include(a => a.TaskDetails).Include(a => a.TaskEmailRecipients).Include(a => a.Activity).Where(a => a.TaskHeaderId == taskHeaderId).OrderBy(a => a).FirstOrDefaultAsync();
#pragma warning restore CS8603 // Possible null reference return.
        }

        [HttpGet]
        public async Task<bool> GetTaskHasDetailsAsync(int taskHeaderId)
        {
            return await _context.TaskHeader.Include(a => a.TaskDetails).OrderBy(a => a).Select(a => a.TaskDetails).AnyAsync();
        }

        [HttpPost]
        public async Task<IActionResult> TaskHeaderInsert(ApiCrudPayload<TaskHeader> values)
        {
            try
            {
                if (_context != null)
                {
#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.TaskHeader?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Entry<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.TaskHeader?' doesn't match 'class' constraint.
                    _context.Entry(values.EntityValue).State = EntityState.Added;
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.TaskHeader?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Entry<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.TaskHeader?' doesn't match 'class' constraint.
                    await SaveDbAsync(values.UserName);
#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.TaskHeader?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Entry<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.TaskHeader?' doesn't match 'class' constraint.
                    _context.Entry(values.EntityValue).State = EntityState.Detached;
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.TaskHeader?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Entry<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.TaskHeader?' doesn't match 'class' constraint.
                }

                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TaskHeaderDelete(ApiCrudPayload<TaskHeader> values)
        {
            try
            {
#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.TaskHeader?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Entry<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.TaskHeader?' doesn't match 'class' constraint.
                _context.Entry(values.EntityValue).State = EntityState.Deleted;
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.TaskHeader?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Entry<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.TaskHeader?' doesn't match 'class' constraint.
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TaskClone(ApiCrudPayload<DuplicateTask> values)
        {
            try
            {

                var dbItem = await _context.TaskHeader.Include(a => a.Activity).Include(a => a.TaskDetails).Include(a => a.TaskEmailRecipients).Where(a => a.TaskHeaderId == values.EntityValue.TaskHeaderId).AsNoTracking().FirstOrDefaultAsync();

                if (dbItem != null)
                {
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
                return NotFound(new SubmitResult { Success = false, Message = "Item not found" });
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
#pragma warning disable CS8601 // Possible null reference assignment.

                values.EntityValue.Activity = await _context.Activity.Where(a => a.ActivityId == values.EntityValue.IdActivity).FirstOrDefaultAsync();

#pragma warning restore CS8601 // Possible null reference assignment.
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
            try
            {
#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.TaskDetail?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Entry<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.TaskDetail?' doesn't match 'class' constraint.
                _context.Entry(values.EntityValue).State = EntityState.Deleted;
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.TaskDetail?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Entry<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.TaskDetail?' doesn't match 'class' constraint.
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SftpInsert(ApiCrudPayload<SftpConfiguration> values)
        {
            try
            {
#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.SftpConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.AddAsync<TEntity>(TEntity, CancellationToken)'. Nullability of type argument 'Report_App_WASM.Server.Models.SftpConfiguration?' doesn't match 'class' constraint.
                await _context.AddAsync(values.EntityValue);
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.SftpConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.AddAsync<TEntity>(TEntity, CancellationToken)'. Nullability of type argument 'Report_App_WASM.Server.Models.SftpConfiguration?' doesn't match 'class' constraint.
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SftpDelete(ApiCrudPayload<SftpConfiguration> values)
        {
            try
            {
#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.SftpConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Remove<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.SftpConfiguration?' doesn't match 'class' constraint.
                _context.Remove(values.EntityValue);
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.SftpConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Remove<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.SftpConfiguration?' doesn't match 'class' constraint.
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SftpUpdate(ApiCrudPayload<SftpConfiguration> values)
        {
            try
            {
#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.SftpConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Update<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.SftpConfiguration?' doesn't match 'class' constraint.
                _context.Update(values.EntityValue);
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.SftpConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Update<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.SftpConfiguration?' doesn't match 'class' constraint.
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DepositPathInsert(ApiCrudPayload<FileDepositPathConfiguration> values)
        {
            try
            {
#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.FileDepositPathConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.AddAsync<TEntity>(TEntity, CancellationToken)'. Nullability of type argument 'Report_App_WASM.Server.Models.FileDepositPathConfiguration?' doesn't match 'class' constraint.
                await _context.AddAsync(values.EntityValue);
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.FileDepositPathConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.AddAsync<TEntity>(TEntity, CancellationToken)'. Nullability of type argument 'Report_App_WASM.Server.Models.FileDepositPathConfiguration?' doesn't match 'class' constraint.
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DepositPathDelete(ApiCrudPayload<FileDepositPathConfiguration> values)
        {
            try
            {
#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.FileDepositPathConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Remove<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.FileDepositPathConfiguration?' doesn't match 'class' constraint.
                _context.Remove(values.EntityValue);
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.FileDepositPathConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Remove<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.FileDepositPathConfiguration?' doesn't match 'class' constraint.
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DepositPathUpdate(ApiCrudPayload<FileDepositPathConfiguration> values)
        {
            try
            {
#pragma warning disable CS8634 // The type 'Report_App_WASM.Server.Models.FileDepositPathConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Update<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.FileDepositPathConfiguration?' doesn't match 'class' constraint.
                _context.Update(values.EntityValue);
#pragma warning restore CS8634 // The type 'Report_App_WASM.Server.Models.FileDepositPathConfiguration?' cannot be used as type parameter 'TEntity' in the generic type or method 'DbContext.Update<TEntity>(TEntity)'. Nullability of type argument 'Report_App_WASM.Server.Models.FileDepositPathConfiguration?' doesn't match 'class' constraint.
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        private async Task SaveDbAsync(string? userId = "system")
        {
            await _context.SaveChangesAsync(userId);
        }
    }
}
