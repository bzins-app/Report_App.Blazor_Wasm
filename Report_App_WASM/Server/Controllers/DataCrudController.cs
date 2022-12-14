using AutoMapper;
using Community.OData.Linq;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI.Common;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Utils;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.ApiResponse;
using Report_App_WASM.Shared.DTO;
using System.Threading.Tasks;
using static MudBlazor.CategoryTypes;

namespace Report_App_WASM.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
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

        [HttpGet("LogTaskDetails")]
        public async Task<IEnumerable<ApplicationLogTaskDetails>> GetLogTaskDetailsAsync(int LogTaskHeader)
        {
           return  await _context.ApplicationLogTaskDetails.Where(a => a.TaskId == LogTaskHeader).ToArrayAsync();
        }

        [HttpGet("ActivityDbConnection")]
        public async Task<IEnumerable<ActivityDbConnection>> GetActivityDbConnectionAsync(int ActivityId)
        {
            return await _context.ActivityDbConnection.Include(a=>a.Activity).Where(a => a.Activity.ActivityId == ActivityId).ToArrayAsync();
        }


        [HttpPost]
        public async Task<IActionResult> InsertSMTP(ApiCRUDPayload<SMTPConfiguration> values)
        {
            try
            {
                await _context.AddAsync(values.EntityValue);
                await SaveDbAsync(values.UserName);
                return Ok();
            }
            catch (Exception ex)
            {
               return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSMTP(ApiCRUDPayload<SMTPConfiguration> values)
        {
            try
            {
                _context.Remove(values.EntityValue);
                await SaveDbAsync(values.UserName);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSMTP(ApiCRUDPayload<SMTPConfiguration> values)
        {
            try
            {
                _context.Update(values.EntityValue);
                await SaveDbAsync(values.UserName);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> InsertLDAP(ApiCRUDPayload<LDAPConfiguration> values)
        {
            try
            {
                await _context.AddAsync(values.EntityValue);
                await SaveDbAsync(values.UserName);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLDAP(ApiCRUDPayload<LDAPConfiguration> values)
        {
            try
            {
                _context.Remove(values.EntityValue);
                await SaveDbAsync(values.UserName);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLDAP(ApiCRUDPayload<LDAPConfiguration> values)
        {
            try
            {
                _context.Update(values.EntityValue);
                await SaveDbAsync(values.UserName);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> InsertActivity(ApiCRUDPayload<Activity> values)
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
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteActivity(ApiCRUDPayload<Activity> values)
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
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateActivity(ApiCRUDPayload<Activity> values)
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

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task SaveDbAsync(string userId="system")
        {
            await _context.SaveChangesAsync(userId);
        }
    }
}
