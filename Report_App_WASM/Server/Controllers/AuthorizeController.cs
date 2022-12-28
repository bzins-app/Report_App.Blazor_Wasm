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
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;

namespace Report_App_WASM.Server.Controllers
{
    // [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthorizeController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthorizeController> _logger;
        private readonly IBackgroundWorkers _emailSender;

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
            var user = await _userManager.FindByNameAsync(parameters.UserName!);
            if (user == null) return BadRequest("User does not exist");
            var singInResult = await _signInManager.CheckPasswordSignInAsync(user, parameters.Password!, false);
            if (!singInResult.Succeeded) return BadRequest("Invalid password");

            await _signInManager.SignInAsync(user, parameters.RememberMe);

            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> LdapLogin(LoginParameters parameters)
        {
            var domain = await _context.LdapConfiguration.Where(a => a.IsActivated).Select(a => a.Domain).FirstOrDefaultAsync();
            try
            {
                var rememberMe = true;
                using var context = new PrincipalContext(ContextType.Domain, domain, parameters.UserName, parameters.Password);
                var userAd = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, parameters.UserName!);
                var userMail = await _userManager.FindByEmailAsync(userAd!.EmailAddress);
                if (userMail != null)
                {
                    await _signInManager.SignInAsync(userMail, rememberMe);
                    _logger.LogInformation("User logged in:" + parameters.UserName);
                    return Ok();

                }

                var user = await _userManager.FindByNameAsync(parameters.UserName!);
                if (user != null)
                {
                    await _signInManager.SignInAsync(user, rememberMe);
                    _logger.LogInformation("User logged in:" + parameters.UserName);
                    return Ok();
                }

                List<string> errors = new();
                var userNew = new ApplicationUser { UserName = parameters.UserName, Email = userAd.EmailAddress, CreateUser = "AD screen", ModDateTime = DateTime.Now, ModificationUser = "Register screen", Culture = CultureInfo.CurrentCulture.Name, EmailConfirmed = true };
                var result = await _userManager.CreateAsync(userNew).ConfigureAwait(true);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(userNew, rememberMe);
                    return Ok();
                }
                foreach (var error in result.Errors)
                {
                    errors.Add(error.Description);
                }
                if (!result.Succeeded)
                {
                    return BadRequest(string.Join(',', errors));
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Invalid login attempt during AD auth " + parameters.UserName + " Error:" + ex.Message);
                ModelState.AddModelError(string.Empty, ex.Message);
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterParameters parameters)
        {
            var user = new ApplicationUser();
            user.UserName = parameters.UserName;
            if (parameters.Password != null)
            {
                var result = await _userManager.CreateAsync(user, parameters.Password);
                if (!result.Succeeded) return BadRequest(result.Errors.FirstOrDefault()?.Description);
            }

            return await Login(new()
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
            try
            {
                if (item != null)
                {
                    var user = await _userManager.FindByEmailAsync(item.EntityValue.UserMail!);
                    await _userManager.ResetPasswordAsync(user!, item.EntityValue.UserName!, item.EntityValue.Password!);
                }
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
            try
            {
                if (item != null)
                {
                    var user = await _userManager.FindByEmailAsync(item.EntityValue.UserMail!).ConfigureAwait(true);
                    if (user == null || !(await _userManager.IsEmailConfirmedAsync(user).ConfigureAwait(true)))
                    {
                        // Don't reveal that the user does not exist or is not confirmed
                        return RedirectToPage("./ForgotPasswordConfirmation");
                    }

                    // For more information on how to enable account confirmation and password reset please 
                    // visit https://go.microsoft.com/fwlink/?LinkID=532713
                    var code = await _userManager.GeneratePasswordResetTokenAsync(user).ConfigureAwait(true);
                    var callbackUrl = Url.Page(
                        "/ResetPassword",
                        pageHandler: null,
                        values: new { code },
                        protocol: Request.Scheme);

                    List<EmailRecipient> ListEmail = new()
                {
                    new EmailRecipient { Email = item.EntityValue.UserMail }
                };
                    string Title = "Reset Password";
                    string Body = $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.";

                    _emailSender.SendEmail(ListEmail, Title, Body);
                }
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
            var user = await _userManager.FindByIdAsync(value.EntityValue.UserId!).ConfigureAwait(true);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{value.EntityValue.UserId!}'.");
            }

            var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(value.EntityValue.Code!));
            var result = await _userManager.ConfirmEmailAsync(user, code).ConfigureAwait(true);
            return Ok(new SubmitResult { Success = result.Succeeded, Message = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email." });
        }

        [HttpGet]
        public async Task<UserInfo> UserInfoAsync()
        {
            //var user = await _userManager.GetUserAsync(HttpContext.User);
            return await BuildUserInfoAsync();
        }

        [HttpGet]
        public IdentityDefaultOptions GetIdentityOptionsAsync()
        {
            //var user = await _userManager.GetUserAsync(HttpContext.User);
            var identityDefaultOptions = new IdentityDefaultOptions();
            var options = _userManager.Options;

            identityDefaultOptions.PasswordRequireDigit = options.Password.RequireDigit;
            identityDefaultOptions.PasswordRequiredLength = options.Password.RequiredLength;
            identityDefaultOptions.PasswordRequireNonAlphanumeric = options.Password.RequireNonAlphanumeric;
            identityDefaultOptions.PasswordRequireUppercase = options.Password.RequireUppercase;
            identityDefaultOptions.PasswordRequireLowercase = options.Password.RequireLowercase;
            identityDefaultOptions.PasswordRequiredUniqueChars = options.Password.RequiredUniqueChars;

            // Lockout settings
            identityDefaultOptions.LockoutDefaultLockoutTimeSpanInMinutes = options.Lockout.DefaultLockoutTimeSpan.Minutes;
            identityDefaultOptions.LockoutMaxFailedAccessAttempts = options.Lockout.MaxFailedAccessAttempts;
            identityDefaultOptions.LockoutAllowedForNewUsers = options.Lockout.AllowedForNewUsers;

            // User settings
            identityDefaultOptions.UserRequireUniqueEmail = options.User.RequireUniqueEmail;

            // email confirmation require
            identityDefaultOptions.SignInRequireConfirmedEmail = options.SignIn.RequireConfirmedEmail;
            return identityDefaultOptions;
        }

        private async Task<UserInfo> BuildUserInfoAsync()
        {
            var userData = await _userManager.GetUserAsync(User);
            return new()
            {
                IsAuthenticated = User.Identity!.IsAuthenticated,
                UserName = User.Identity.Name,
                UserMail = userData?.Email,
                AppTheme = userData?.ApplicationTheme ?? "Light",
                Culture = userData?.Culture ?? "en",
                ExposedClaims = User.Claims
                    //Optionally: filter the claims you want to expose to the client
                    //.Where(c => c.Type == "role")
                    .Select(c => new ClaimsValue { Type = c.Type, Value = c.Value }).ToList()
            };
        }
    }
}
