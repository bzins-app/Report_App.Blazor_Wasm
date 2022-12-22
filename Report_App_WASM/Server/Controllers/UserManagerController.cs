using AutoMapper;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Shared;
using System.Text.Encodings.Web;
using System.Text;
using ReportAppWASM.Server.Services.BackgroundWorker;
using Report_App_WASM.Client.Pages.UserManager;
using static MudBlazor.CategoryTypes;
using Microsoft.AspNetCore.Authorization;

namespace Report_App_WASM.Server.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize]
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class UserManagerController : ControllerBase
    {
        private readonly ILogger<UserManagerController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBackgroundWorkers _backgroundWorker;

        public UserManagerController(ILogger<UserManagerController> logger,
            ApplicationDbContext context, IMapper mapper,
             RoleManager<IdentityRole<Guid>> roleManager, UserManager<ApplicationUser> userManager, IBackgroundWorkers backgroundWorker)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _roleManager = roleManager;
            _userManager = userManager;
            _backgroundWorker = backgroundWorker;
        }

        [HttpGet]
        public async Task<IEnumerable<string>> GetRolesListAsync()
        {
           return await _roleManager.Roles.Select(a => a.Name).ToListAsync();
        }
        [HttpGet]
        public async Task<IEnumerable<string>> GetRolesListPerUserAsync(string UserName)
        {
            List<string> userRoles= new List<string>();
            var user= await  _userManager.FindByNameAsync(UserName);
            if(user==null)
            {
                return userRoles;
            }
            var roles= await _context.UserRoles.Where(a => a.UserId == user.Id).Select(a => a.RoleId).ToListAsync();
            foreach(var role in roles)
            {
                userRoles.Add(await _roleManager.Roles.Where(a => a.Id == role).Select(a => a.Name).FirstOrDefaultAsync());
            }
            return userRoles;
        }

        [HttpPost]
        public async Task<IActionResult> UserInsert(ApiCRUDPayload<UserPayload> item)
        {
            try
            {
                var appUser = new ApplicationUser();
                appUser.CreateDateTime = DateTime.Now;
                appUser.CreateUser = item.UserName;
                appUser.ModDateTime = DateTime.Now;
                appUser.ModificationUser = item.UserName;
                appUser.Email = item.EntityValue.UserMail;
                appUser.UserName = item.EntityValue.UserName;
                var password = item.EntityValue.Password;
                appUser.PasswordHash = "";
                var result = await _userManager.CreateAsync(appUser, password);
                if (result.Succeeded)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(appUser); ;
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = appUser.Id, code },
                        protocol: Request.Scheme);
                    List<EmailRecipient> ListEmail = new();
                    var emailPrefix = await _context.ApplicationParameters.Select(a => a.EmailPrefix).FirstOrDefaultAsync();
                    ListEmail.Add(new EmailRecipient { Email = appUser.Email });
                    var Title = emailPrefix + " - Confirm your email";
                    var Body = $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.";
                    _backgroundWorker.SendEmail(ListEmail, Title, Body);
                }

                return Ok(new SubmitResult { Success = true, Message = "Ok" });
            }
            catch (Exception e)
            {
                return Ok(new SubmitResult { Success = false, Message = e.Message });
            }

        }
        [HttpPost]
        public async Task<IActionResult> AddRolesAsync(ApiCRUDPayload<ChangeRolePayload> item)
        {
            var user = await _userManager.FindByNameAsync(item.EntityValue.UserName);
            var result = await _userManager.AddToRolesAsync(user, item.EntityValue.Roles);
            // await _SignIn.RefreshSignInAsync(user);
            _logger.Log(LogLevel.Warning, $"User {item.EntityValue.UserName} get new roles by {item.UserName} " + string.Join(",", item.EntityValue.Roles));
            return Ok(new SubmitResult { Success = result.Succeeded});
        }
        [HttpPost]
        public async Task<IActionResult> RemoveRolesAsync(ApiCRUDPayload<ChangeRolePayload> item)
        {
            var user = await _userManager.FindByNameAsync(item.EntityValue.UserName);
            var result = await _userManager.RemoveFromRolesAsync(user, item.EntityValue.Roles);
            // await _SignIn.RefreshSignInAsync(user);
            _logger.Log(LogLevel.Warning, $"User {item.EntityValue.UserName} has been removed from roles by {item.UserName} " + string.Join(",", item.EntityValue.Roles));
            return Ok(new SubmitResult { Success = result.Succeeded });
        }

        [HttpPost]
        public async Task<IActionResult> UserDelete(ApiCRUDPayload<UserPayload> item)
        {
            try
            {
                if (item != null)
                {
                    var user = await _userManager.FindByNameAsync(item.EntityValue.UserName);
                    await _userManager.DeleteAsync(user);
                }
                return Ok(new SubmitResult { Success = true, Message = "Ok" });
            }
            catch (Exception e)
            {
                return Ok(new SubmitResult { Success = false, Message = e.Message });
            }
        }

    }
}
