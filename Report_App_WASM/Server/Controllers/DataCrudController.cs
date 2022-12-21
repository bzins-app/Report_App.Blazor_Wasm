using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Utils;
using Report_App_WASM.Shared;

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
        public async Task<IEnumerable<ApplicationLogTaskDetails>> GetLogTaskDetailsAsync(int LogTaskHeaderId)
        {
            return await _context.ApplicationLogTaskDetails.Where(a => a.TaskId == LogTaskHeaderId).ToArrayAsync();
        }

        [HttpGet]
        public async Task<IEnumerable<ActivityDbConnection>> GetActivityDbConnectionAsync(int ActivityId)
        {
            return await _context.ActivityDbConnection.Where(a => a.Activity.ActivityId == ActivityId).ToArrayAsync();
        }

		[HttpGet]
		public async Task<SFTPConfiguration> GetSTFPConfigurationAsync(int SFTPConfigurationId)
		{
			return await _context.SFTPConfiguration.Where(a => a.SFTPConfigurationId == SFTPConfigurationId).FirstOrDefaultAsync();
		}

        [HttpGet]
        public async Task<Activity> GetDataTransferInfoAsync()
        {
            return await _context.Activity.Where(a => a.ActivityType == ActivityType.TargetDB.ToString()).Include(a=>a.ActivityDbConnections).FirstOrDefaultAsync();
        }

        [HttpGet]
        public async Task<ServicesStatus> GetServiceStatusAsync()
        {
            return await _context.ServicesStatus.OrderBy(a => a.Id).FirstOrDefaultAsync();
        }

        [HttpPost]
        public async Task<IActionResult> ApplicationParametersUpdateAsync(ApiCRUDPayload<ApplicationParameters> values)
        {
            try
            {
                _context.Update(values.EntityValue);
                await SaveDbAsync(values.UserName);
                ApplicationConstants.ApplicationName= values.EntityValue.ApplicationName;
                ApplicationConstants.ApplicationLogo = values.EntityValue.ApplicationLogo;
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SMTPInsert(ApiCRUDPayload<SMTPConfiguration> values)
        {
            try
            {
                await _context.AddAsync(values.EntityValue);
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SMTPDelete(ApiCRUDPayload<SMTPConfiguration> values)
        {
            try
            {
                _context.Remove(values.EntityValue);
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SMTPUpdate(ApiCRUDPayload<SMTPConfiguration> values)
        {
            try
            {
                _context.Update(values.EntityValue);
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SMTPActivate(ApiCRUDPayload<SMTPConfiguration> values)
        {
            try
            {
                var updateValues = values.EntityValue;
                if (updateValues.IsActivated)
                {
                    var others = await _context.SMTPConfiguration.Where(a => a.Id != updateValues.Id).OrderBy(a => a.Id).ToListAsync();
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
        public async Task<IActionResult> LDAPActivate(ApiCRUDPayload<LDAPConfiguration> values)
        {
            try
            {
                var updateValues = values.EntityValue;
                if (updateValues.IsActivated)
                {
                    var others = await _context.LDAPConfiguration.Where(a => a.Id != updateValues.Id).OrderBy(a => a.Id).ToListAsync();
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
        public async Task<IActionResult> LDAPInsert(ApiCRUDPayload<LDAPConfiguration> values)
        {
            try
            {
                await _context.AddAsync(values.EntityValue);
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> LDAPDelete(ApiCRUDPayload<LDAPConfiguration> values)
        {
            try
            {
                _context.Remove(values.EntityValue);
                if (values.EntityValue.IsActivated)
                {
                    ApplicationConstants.LDAPLogin = false;
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
        public async Task<IActionResult> LDAPUpdate(ApiCRUDPayload<LDAPConfiguration> values)
        {
            try
            {
                _context.Update(values.EntityValue);
                await SaveDbAsync(values.UserName);
                ApplicationConstants.LDAPLogin = values.EntityValue.IsActivated;
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ActivityInsert(ApiCRUDPayload<Activity> values)
        {
            try
            {
                if (!await _roleManager.RoleExistsAsync(values.EntityValue.ActivityName))
                {
                    await _roleManager.CreateAsync(new IdentityRole<Guid>(values.EntityValue.ActivityName));
                    var users = await _userManager.GetUsersInRoleAsync("Admin");

                    foreach (var user in users)
                    {
                        if (!await _userManager.IsInRoleAsync(user, values.EntityValue.ActivityName))
                        {
                            var roleresult = await _userManager.AddToRoleAsync(user, values.EntityValue.ActivityName);
                            // await _SignIn.RefreshSignInAsync(user);
                        }
                    }
                }
                if (string.IsNullOrEmpty(values.EntityValue.ActivityRoleId))
                {
                    var newRole = await _roleManager.FindByNameAsync(values.EntityValue.ActivityName);
                    values.EntityValue.ActivityRoleId = newRole.Id.ToString();
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
        public async Task<IActionResult> ActivityDelete(ApiCRUDPayload<Activity> values)
        {
            try
            {
                var role = await _roleManager.FindByNameAsync(values.EntityValue.ActivityName);
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
        public async Task<IActionResult> ActivityUpdate(ApiCRUDPayload<Activity> values)
        {
            try
            {
                var roleActivity = await _roleManager.FindByIdAsync(values.EntityValue.ActivityRoleId);
                if (roleActivity != null)
                {
                    if (roleActivity.Name != values.EntityValue.ActivityName)
                    {
                        roleActivity.Name = values.EntityValue.ActivityName;
                        await _roleManager.UpdateAsync(roleActivity);
                    }
                }

                _context.Update(values.EntityValue);
                await SaveDbAsync(values.UserName);
                _context.Entry(values.EntityValue).State = EntityState.Detached;

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
            return await _context.TaskHeader.Include(a => a.TaskDetails).Include(a => a.TaskEmailRecipients).Include(a => a.Activity).Where(a => a.TaskHeaderId == taskHeaderId).OrderBy(a => a).FirstOrDefaultAsync();
        }

        [HttpPost]
        public async Task<IActionResult> TaskHeaderInsert(ApiCRUDPayload<TaskHeader> values)
        {
            try
            {
                _context.Entry(values.EntityValue).State = EntityState.Added;
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
        public async Task<IActionResult> TaskHeaderDelete(ApiCRUDPayload<TaskHeader> values)
        {
            try
            {
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
        public async Task<IActionResult> TaskHeaderUpdate(ApiCRUDPayload<TaskHeader> values)
        {
            try
            {
                values.EntityValue.Activity = await _context.Activity.Where(a => a.ActivityId == values.EntityValue.IdActivity).FirstOrDefaultAsync();
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
        public async Task<IActionResult> TaskDetailDelete(ApiCRUDPayload<TaskDetail> values)
        {
            try
            {
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
        public async Task<IActionResult> SFTPInsert(ApiCRUDPayload<SFTPConfiguration> values)
        {
            try
            {
                await _context.AddAsync(values.EntityValue);
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SFTPDelete(ApiCRUDPayload<SFTPConfiguration> values)
        {
            try
            {
                _context.Remove(values.EntityValue);
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SFTPUpdate(ApiCRUDPayload<SFTPConfiguration> values)
        {
            try
            {
                _context.Update(values.EntityValue);
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DepositPathInsert(ApiCRUDPayload<FileDepositPathConfiguration> values)
        {
            try
            {
                await _context.AddAsync(values.EntityValue);
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DepositPathDelete(ApiCRUDPayload<FileDepositPathConfiguration> values)
        {
            try
            {
                _context.Remove(values.EntityValue);
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DepositPathUpdate(ApiCRUDPayload<FileDepositPathConfiguration> values)
        {
            try
            {
                _context.Update(values.EntityValue);
                await SaveDbAsync(values.UserName);
                return Ok(new SubmitResult { Success = true });
            }
            catch (Exception ex)
            {
                return Ok(new SubmitResult { Success = false, Message = ex.Message });
            }
        }

        private async Task SaveDbAsync(string userId = "system")
        {
            await _context.SaveChangesAsync(userId);
        }
    }
}
