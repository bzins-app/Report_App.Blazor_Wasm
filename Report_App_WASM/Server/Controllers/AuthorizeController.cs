using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.WebUtilities;

namespace Report_App_WASM.Server.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Route("api/[controller]/[action]")]
[ApiController]
public class AuthorizeController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IBackgroundWorkers _emailSender;
    private readonly ILogger<AuthorizeController> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthorizeController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager, ApplicationDbContext context,
        ILogger<AuthorizeController> logger, IBackgroundWorkers emailSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _logger = logger;
        _emailSender = emailSender;
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginParameters parameters)
    {
        var user = await _userManager.FindByNameAsync(parameters.UserName!)
                   ?? await _userManager.FindByEmailAsync(parameters.UserName!);
        if (user == null)
            return BadRequest("User does not exist");

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, parameters.Password!, false);
        if (!signInResult.Succeeded) return BadRequest("Invalid password");

        await _signInManager.SignInAsync(user, parameters.RememberMe);
        _logger.LogInformation("User logged in: {UserName}", parameters.UserName);
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> LdapLogin(LoginParameters parameters)
    {
        var domain = await _context.LdapConfiguration
            .Where(a => a.IsActivated)
            .Select(a => a.Domain)
            .FirstOrDefaultAsync();

        try
        {
            var rememberMe = true;
            using var context = new PrincipalContext(ContextType.Domain, domain, parameters.UserName, parameters.Password);
            var userAd = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, parameters.UserName!);
            var userMail = await _userManager.FindByEmailAsync(userAd!.EmailAddress);

            if (userMail != null)
            {
                await _signInManager.SignInAsync(userMail, rememberMe);
                _logger.LogInformation("User logged in: {UserName}", parameters.UserName);
                return Ok();
            }

            var user = await _userManager.FindByNameAsync(parameters.UserName!);
            if (user != null)
            {
                await _signInManager.SignInAsync(user, rememberMe);
                _logger.LogInformation("User logged in: {UserName}", parameters.UserName);
                return Ok();
            }

            var userNew = new ApplicationUser
            {
                UserName = parameters.UserName,
                Email = userAd.EmailAddress,
                CreateUser = "AD screen",
                ModDateTime = DateTime.Now,
                ModificationUser = "Register screen",
                Culture = CultureInfo.CurrentCulture.Name,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(userNew);
        
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(userNew, rememberMe);
                _logger.LogInformation("User logged in: {UserName}", parameters.UserName);
                return Ok();
            }

            var errors = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(string.Join(',', errors));
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Invalid login attempt during AD auth {UserName} Error: {ErrorMessage}", parameters.UserName, ex.Message);
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Register(RegisterParameters parameters)
    {
        var user = new ApplicationUser
        {
            UserName = parameters.UserName
        };

        if (parameters.Password != null)
        {
            var result = await _userManager.CreateAsync(user, parameters.Password);
            if (!result.Succeeded) return BadRequest(result.Errors.FirstOrDefault()?.Description);
        }

        return await Login(new LoginParameters
        {
            UserName = parameters.UserName,
            Password = parameters.Password
        });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ApiCrudPayload<UserPayload> item)
    {
        if (item == null) return BadRequest("Invalid request");

        try
        {
            var user = await _userManager.FindByEmailAsync(item.EntityValue.UserMail!);
            if (user == null) return BadRequest("User not found");

            await _userManager.ResetPasswordAsync(user, item.EntityValue.UserName!, item.EntityValue.Password!);
            return Ok(new SubmitResult { Success = true, Message = "Ok" });
        }
        catch (Exception e)
        {
            return Ok(new SubmitResult { Success = false, Message = e.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SendResetPasswordEmail(ApiCrudPayload<UserPayload> item)
    {
        if (item == null) return BadRequest("Invalid request");

        try
        {
            var user = await _userManager.FindByEmailAsync(item.EntityValue.UserMail!);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                return RedirectToPage("./ForgotPasswordConfirmation");

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Page("/ResetPassword", null, new { code }, Request.Scheme);

            var emailRecipients = new List<EmailRecipient> { new EmailRecipient { Email = item.EntityValue.UserMail } };
            var title = "Reset Password";
            var body = $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.";

            _emailSender.SendEmail(emailRecipients, title, body);
            return Ok(new SubmitResult { Success = true, Message = "Ok" });
        }
        catch (Exception e)
        {
            return Ok(new SubmitResult { Success = false, Message = e.Message });
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ConfirmEmail(ApiCrudPayload<ConfirmEmailValues> value)
    {
        var user = await _userManager.FindByIdAsync(value.EntityValue.UserId!);
        if (user == null) return NotFound($"Unable to load user with ID '{value.EntityValue.UserId!}'.");

        var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(value.EntityValue.Code!));
        var result = await _userManager.ConfirmEmailAsync(user, code);
        return Ok(new SubmitResult
        {
            Success = result.Succeeded,
            Message = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email."
        });
    }

    [HttpGet]
    public async Task<UserInfo> UserInfoAsync()
    {
        return await BuildUserInfoAsync();
    }

    [HttpGet]
    public IdentityDefaultOptions GetIdentityOptionsAsync()
    {
        var options = _userManager.Options;
        return new IdentityDefaultOptions
        {
            PasswordRequireDigit = options.Password.RequireDigit,
            PasswordRequiredLength = options.Password.RequiredLength,
            PasswordRequireNonAlphanumeric = options.Password.RequireNonAlphanumeric,
            PasswordRequireUppercase = options.Password.RequireUppercase,
            PasswordRequireLowercase = options.Password.RequireLowercase,
            PasswordRequiredUniqueChars = options.Password.RequiredUniqueChars,
            LockoutDefaultLockoutTimeSpanInMinutes = options.Lockout.DefaultLockoutTimeSpan.Minutes,
            LockoutMaxFailedAccessAttempts = options.Lockout.MaxFailedAccessAttempts,
            LockoutAllowedForNewUsers = options.Lockout.AllowedForNewUsers,
            UserRequireUniqueEmail = options.User.RequireUniqueEmail,
            SignInRequireConfirmedEmail = options.SignIn.RequireConfirmedEmail
        };
    }

    private async Task<UserInfo> BuildUserInfoAsync()
    {
        var userData = await _userManager.GetUserAsync(User);
        return new UserInfo
        {
            IsAuthenticated = User.Identity!.IsAuthenticated,
            UserName = User.Identity.Name,
            UserMail = userData?.Email,
            AppTheme = userData?.ApplicationTheme ?? "Light",
            Culture = userData?.Culture ?? "en",
            ExposedClaims = User.Claims
                .Select(c => new ClaimsValue { Type = c.Type, Value = c.Value }).ToList()
        };
    }
}
