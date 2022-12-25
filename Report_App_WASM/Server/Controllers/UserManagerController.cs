using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Services.BackgroundWorker;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.ApiExchanges;
using System.Text;
using System.Text.Encodings.Web;

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
            return (await _roleManager.Roles.Select(a => a.Name).ToListAsync())!;
        }
        [HttpGet]
        public async Task<IEnumerable<string>> GetRolesListPerUserAsync(string userName)
        {
            List<string> userRoles = new();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return userRoles;
            }
            var roles = await _context.UserRoles.Where(a => a.UserId == user.Id).Select(a => a.RoleId).ToListAsync();
            foreach (var role in roles)
            {
#pragma warning disable CS8604 // Possible null reference argument for parameter 'item' in 'void List<string>.Add(string item)'.
                userRoles.Add(await _roleManager.Roles.Where(a => a.Id == role).Select(a => a.Name).FirstOrDefaultAsync());
#pragma warning restore CS8604 // Possible null reference argument for parameter 'item' in 'void List<string>.Add(string item)'.
            }
            return userRoles;
        }

        [HttpPost]
        public async Task<IActionResult> UserInsert(ApiCrudPayload<UserPayload> item)
        {
            try
            {
                var appUser = new ApplicationUser();
                appUser.CreateDateTime = DateTime.Now;
                appUser.CreateUser = item.UserName;
                appUser.ModDateTime = DateTime.Now;
                appUser.ModificationUser = item.UserName;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                appUser.Email = item.EntityValue.UserMail;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                appUser.UserName = item.EntityValue.UserName;
                var password = item.EntityValue.Password;
                appUser.PasswordHash = "";
#pragma warning disable CS8604 // Possible null reference argument for parameter 'password' in 'Task<IdentityResult> UserManager<ApplicationUser>.CreateAsync(ApplicationUser user, string password)'.
                var result = await _userManager.CreateAsync(appUser, password);
#pragma warning restore CS8604 // Possible null reference argument for parameter 'password' in 'Task<IdentityResult> UserManager<ApplicationUser>.CreateAsync(ApplicationUser user, string password)'.
                if (result.Succeeded)
                {
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = appUser.Id, code },
                        protocol: Request.Scheme);
                    List<EmailRecipient> listEmail = new();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    var emailPrefix = await _context.ApplicationParameters.Select(a => a.EmailPrefix).FirstOrDefaultAsync();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    listEmail.Add(new() { Email = appUser.Email });
                    var title = emailPrefix + " - Confirm your email";
#pragma warning disable CS8604 // Possible null reference argument for parameter 'value' in 'string TextEncoder.Encode(string value)'.
                    var body = $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.";
#pragma warning restore CS8604 // Possible null reference argument for parameter 'value' in 'string TextEncoder.Encode(string value)'.
                    _backgroundWorker.SendEmail(listEmail, title, body);
                }

                return Ok(new SubmitResult { Success = true, Message = "Ok" });
            }
            catch (Exception e)
            {
                return Ok(new SubmitResult { Success = false, Message = e.Message });
            }

        }
        [HttpPost]
        public async Task<IActionResult> AddRolesAsync(ApiCrudPayload<ChangeRolePayload> item)
        {
            var user = await _userManager.FindByNameAsync(item.EntityValue?.UserName!);
#pragma warning disable CS8604 // Possible null reference argument for parameter 'user' in 'Task<IdentityResult> UserManager<ApplicationUser>.AddToRolesAsync(ApplicationUser user, IEnumerable<string> roles)'.
            var result = await _userManager.AddToRolesAsync(user, item.EntityValue?.Roles!);
#pragma warning restore CS8604 // Possible null reference argument for parameter 'user' in 'Task<IdentityResult> UserManager<ApplicationUser>.AddToRolesAsync(ApplicationUser user, IEnumerable<string> roles)'.
            // await _SignIn.RefreshSignInAsync(user);
#pragma warning disable CS8604 // Possible null reference argument for parameter 'values' in 'string string.Join(string? separator, IEnumerable<string?> values)'.
            _logger.Log(LogLevel.Warning, $"User {item.EntityValue!.UserName} get new roles by {item.UserName} " + string.Join(",", item.EntityValue.Roles));
#pragma warning restore CS8604 // Possible null reference argument for parameter 'values' in 'string string.Join(string? separator, IEnumerable<string?> values)'.
            return Ok(new SubmitResult { Success = result.Succeeded });
        }
        [HttpPost]
        public async Task<IActionResult> RemoveRolesAsync(ApiCrudPayload<ChangeRolePayload> item)
        {
            var user = await _userManager.FindByNameAsync(item.EntityValue?.UserName!);
#pragma warning disable CS8604 // Possible null reference argument for parameter 'user' in 'Task<IdentityResult> UserManager<ApplicationUser>.RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles)'.
            var result = await _userManager.RemoveFromRolesAsync(user, item.EntityValue?.Roles!);
#pragma warning restore CS8604 // Possible null reference argument for parameter 'user' in 'Task<IdentityResult> UserManager<ApplicationUser>.RemoveFromRolesAsync(ApplicationUser user, IEnumerable<string> roles)'.
            // await _SignIn.RefreshSignInAsync(user);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument for parameter 'values' in 'string string.Join(string? separator, IEnumerable<string?> values)'.
            _logger.Log(LogLevel.Warning, $"User {item.EntityValue?.UserName} has been removed from roles by {item.UserName} " + string.Join(",", item.EntityValue.Roles));
#pragma warning restore CS8604 // Possible null reference argument for parameter 'values' in 'string string.Join(string? separator, IEnumerable<string?> values)'.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            return Ok(new SubmitResult { Success = result.Succeeded });
        }

        [HttpPost]
        public async Task<IActionResult> UserDelete(ApiCrudPayload<UserPayload> item)
        {
            try
            {
                if (item != null)
                {
                    var user = await _userManager.FindByNameAsync(item.EntityValue?.UserName!);
#pragma warning disable CS8604 // Possible null reference argument for parameter 'user' in 'Task<IdentityResult> UserManager<ApplicationUser>.DeleteAsync(ApplicationUser user)'.
                    await _userManager.DeleteAsync(user);
#pragma warning restore CS8604 // Possible null reference argument for parameter 'user' in 'Task<IdentityResult> UserManager<ApplicationUser>.DeleteAsync(ApplicationUser user)'.
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
