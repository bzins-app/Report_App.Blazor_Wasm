using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Shared;
using System.DirectoryServices.AccountManagement;
using System.Globalization;

namespace Report_App_WASM.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthorizeController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthorizeController> _logger;

        public AuthorizeController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, ApplicationDbContext context,
            ILogger<AuthorizeController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginParameters parameters)
        {
            var user = await _userManager.FindByNameAsync(parameters.UserName);
            if (user == null) return BadRequest("User does not exist");
            var singInResult = await _signInManager.CheckPasswordSignInAsync(user, parameters.Password, false);
            if (!singInResult.Succeeded) return BadRequest("Invalid password");

            await _signInManager.SignInAsync(user, parameters.RememberMe);

            return Ok();
        }


        [HttpPost]
        public async Task<IActionResult> LDAPLogin(LoginParameters parameters)
        {
            var domain = await _context.LDAPConfiguration.Where(a => a.IsActivated).Select(a => a.Domain).FirstOrDefaultAsync();
            try
            {
                var RememberMe = true;
                using var context = new PrincipalContext(ContextType.Domain, domain, parameters.UserName, parameters.Password);
                UserPrincipal userAD = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, parameters.UserName);
                var userMail = await _userManager.FindByEmailAsync(userAD.EmailAddress);
                if (userMail != null)
                {
                    await _signInManager.SignInAsync(userMail, RememberMe);
                    _logger.LogInformation("User logged in:" + parameters.UserName);
                    return Ok();

                }
                else
                {
                    var user = await _userManager.FindByNameAsync(parameters.UserName);
                    if (user != null)
                    {
                        await _signInManager.SignInAsync(user, RememberMe);
                        _logger.LogInformation("User logged in:" + parameters.UserName);
                        return Ok();
                    }
                    else
                    {
                        List<string> errors = new List<string>();
                        var userNew = new ApplicationUser { UserName = parameters.UserName, Email = userAD.EmailAddress, CreateUser = "AD screen", ModDateTime = DateTime.Now, ModificationUser = "Register screen", Culture = CultureInfo.CurrentCulture.Name, EmailConfirmed = true };
                        var result = await _userManager.CreateAsync(userNew).ConfigureAwait(true);
                        if (result.Succeeded)
                        {
                            await _signInManager.SignInAsync(userNew, RememberMe);
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


        [HttpPost]
        public async Task<IActionResult> Register(RegisterParameters parameters)
        {
            var user = new ApplicationUser();
            user.UserName = parameters.UserName;
            var result = await _userManager.CreateAsync(user, parameters.Password);
            if (!result.Succeeded) return BadRequest(result.Errors.FirstOrDefault()?.Description);

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

        [HttpGet]
        public async Task<UserInfo> UserInfoAsync()
        {
            //var user = await _userManager.GetUserAsync(HttpContext.User);
            return await BuildUserInfoAsync();
        }


        private async Task<UserInfo> BuildUserInfoAsync()
        {
            var userData = await _userManager.GetUserAsync(User);
            return new UserInfo
            {
                IsAuthenticated = User.Identity.IsAuthenticated,
                UserName = User.Identity.Name,
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
