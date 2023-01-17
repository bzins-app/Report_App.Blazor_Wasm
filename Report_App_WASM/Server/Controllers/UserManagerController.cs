using System.Text;
using System.Text.Encodings.Web;
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

namespace Report_App_WASM.Server.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
[Route("api/[controller]/[Action]")]
[ApiController]
public class UserManagerController : ControllerBase, IDisposable
{
    private readonly IBackgroundWorkers _backgroundWorker;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserManagerController> _logger;
    private readonly IMapper _mapper;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserManagerController(ILogger<UserManagerController> logger,
        ApplicationDbContext context, IMapper mapper,
        RoleManager<IdentityRole<Guid>> roleManager, UserManager<ApplicationUser> userManager,
        IBackgroundWorkers backgroundWorker, SignInManager<ApplicationUser> signInManager)
    {
        _logger = logger;
        _context = context;
        _mapper = mapper;
        _roleManager = roleManager;
        _userManager = userManager;
        _backgroundWorker = backgroundWorker;
        _signInManager = signInManager;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    [HttpGet]
    public async Task<IEnumerable<string>> GetRolesListAsync()
    {
        return (await _roleManager.Roles.Select(a => a.Name).ToListAsync())!;
    }

    [HttpGet]
    public async Task<IEnumerable<string?>> GetRolesListPerUserAsync(string userName)
    {
        List<string?> userRoles = new();
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null) return userRoles;
        var roles = await _context.UserRoles.Where(a => a.UserId == user.Id).Select(a => a.RoleId).ToListAsync();
        foreach (var role in roles)
            userRoles.Add(await _roleManager.Roles.Where(a => a.Id == role).Select(a => a.Name).FirstOrDefaultAsync());
        return userRoles;
    }

    [HttpPost]
    public async Task<IActionResult> UserInsert(ApiCrudPayload<UserPayload> item)
    {
        try
        {
            var appUser = new ApplicationUser
            {
                CreateDateTime = DateTime.Now,
                CreateUser = item.UserName,
                ModDateTime = DateTime.Now,
                ModificationUser = item.UserName,
                Email = item.EntityValue.UserMail,
                UserName = item.EntityValue.UserName
            };
            var password = item.EntityValue.Password;
            // appUser.PasswordHash = "";
            var result = await _userManager.CreateAsync(appUser, password!);
            if (result.Succeeded)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);

                var binaryCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    null,
                    new { area = "Identity", userId = appUser.Id, code = binaryCode },
                    Request.Scheme);

                List<EmailRecipient> listEmail = new();
                var emailPrefix = await _context.ApplicationParameters.Select(a => a.EmailPrefix).FirstOrDefaultAsync();
                listEmail.Add(new EmailRecipient { Email = appUser.Email });
                var title = emailPrefix + " - Confirm your email";
                var body =
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.";
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
        var user = await _userManager.FindByNameAsync(item.EntityValue.UserName);
        var result = await _userManager.AddToRolesAsync(user!, item.EntityValue?.Roles!);
        //await _signInManager.RefreshSignInAsync(user);
        _logger.Log(LogLevel.Warning,
            $"User {item.EntityValue!.UserName} get new roles by {item.UserName} " +
            string.Join(",", item.EntityValue.Roles!));
        return Ok(new SubmitResult { Success = result.Succeeded });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveRolesAsync(ApiCrudPayload<ChangeRolePayload> item)
    {
        var user = await _userManager.FindByNameAsync(item.EntityValue?.UserName!);
        var result = await _userManager.RemoveFromRolesAsync(user!, item.EntityValue?.Roles!);
      //  await _signInManager.RefreshSignInAsync(user);
        _logger.Log(LogLevel.Warning,
            $"User {item.EntityValue?.UserName} has been removed from roles by {item.UserName} " +
            string.Join(",", item.EntityValue.Roles!));
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
                await _userManager.DeleteAsync(user!);
            }

            return Ok(new SubmitResult { Success = true, Message = "Ok" });
        }
        catch (Exception e)
        {
            return Ok(new SubmitResult { Success = false, Message = e.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UserUpdate(ApiCrudPayload<UserPayload> item)
    {
        try
        {
            if (item != null)
            {
                var user = await _userManager.FindByNameAsync(item.EntityValue?.UserName!);
                _context.Update(user!);
                await _context.SaveChangesAsync(item.UserName);
            }

            return Ok(new SubmitResult { Success = true, Message = "Ok" });
        }
        catch (Exception e)
        {
            return Ok(new SubmitResult { Success = false, Message = e.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ApiCrudPayload<UserPayload> item)
    {
        try
        {
            if (item != null)
            {
                var user = await _userManager.FindByNameAsync(item.EntityValue?.UserName!);
                await _userManager.ChangePasswordAsync(user!, item.EntityValue.Password!,
                    item.EntityValue.NewPassword!);
            }

            return Ok(new SubmitResult { Success = true, Message = "Ok" });
        }
        catch (Exception e)
        {
            return Ok(new SubmitResult { Success = false, Message = e.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UserDeleteProfile(ApiCrudPayload<UserPayload> item)
    {
        try
        {
            var message = "";
            if (item != null)
            {
                if (string.IsNullOrEmpty(item.EntityValue.Password))
                {
                    message = "Incorrect password";
                    return Ok(new SubmitResult { Success = true, Message = message });
                }

                var user = await _userManager.FindByNameAsync(item.EntityValue?.UserName!);
                if (!await _userManager.CheckPasswordAsync(user!, item.EntityValue.Password))
                {
                    message = "Incorrect password";
                    return Ok(new SubmitResult { Success = true, Message = message });
                }

                message = "";
                var result = await _userManager.DeleteAsync(user!);
                var userId = await _userManager.GetUserIdAsync(user!);
                if (!result.Succeeded)
                {
                    message = $"Unexpected error occurred deleting user with ID '{userId}'.";
                    return Ok(new SubmitResult { Success = true, Message = message });
                }

                await _userManager.DeleteAsync(user!);
            }

            return Ok(new SubmitResult { Success = true, Message = "Ok" });
        }
        catch (Exception e)
        {
            return Ok(new SubmitResult { Success = false, Message = e.Message });
        }
    }
}