using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Utils.SettingsConfiguration;

namespace Report_App_WASM.Server
{
    public class InitializeDatabase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly BaseUser _baseUser;

        public InitializeDatabase(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            ApplicationDbContext context,
            IOptions<BaseUser> baseUser
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _baseUser = baseUser.Value;
        }
        public async Task InitializeAsync()
        {
            try
            {
                _context.Database.Migrate();
                _context.Database.EnsureCreated();

                // check for users
                if (_context.ApplicationUser.Any())
                {
                    await InitAppRoles();
                    return; //if user is not empty, DB has been seed
                }
                else
                {

                    //init app with super admin user
                    await CreateHashKey();
                    await InitAppRoles();
                    await CreateDefaultSuperAdmin();
                    await ApplicationParameters();
                    await DefaultServiceStatus();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task CreateHashKey()
        {
            ApplicationUniqueKey key = new() { Id = Guid.NewGuid() };
            await _context.AddAsync(key);
            await _context.SaveChangesAsync();
        }

        private async Task InitAppRoles()
        {
            try
            {
                List<string> roles = new();
                string[] Role = { "Admin",
                        "Supervisor"};
                roles.AddRange(Role);
                foreach (var t in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(t).ConfigureAwait(true))
                    {
                        await _roleManager.CreateAsync(new IdentityRole<Guid>(t)).ConfigureAwait(true);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task CreateDefaultSuperAdmin()
        {
            try
            {
                ApplicationUser superAdmin = new();

                await _userManager.CreateAsync(
                    new ApplicationUser
                    {
                        Email = _baseUser.Email,
                        UserName = _baseUser.Email,
                        EmailConfirmed = true,
                        IsBaseUser = true,
                        Culture = "en",
                        CreateUser = "DB initialization",
                        ModificationUser = "DB initialization",
                        ModDateTime = System.DateTime.Now,
                        CreateDateTime = DateTime.Now
                    }
                    , _baseUser.Password);

                //loop all the roles and then fill to SuperAdmin so he become powerfull
                ApplicationUser selectedUser = await _userManager.FindByEmailAsync(_baseUser.Email).ConfigureAwait(true);
                List<string> roles = new();
                if (selectedUser != null)
                {
                    roles = _roleManager.Roles.Select(a => a.Name).ToList();
                    await _userManager.AddToRolesAsync(selectedUser, roles).ConfigureAwait(true);

                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task ApplicationParameters()
        {
            try
            {
                ApplicationParameters Parameters = new()
                {
                    ApplicationName = "Report Service App",
                    CreateDateTime = DateTime.Now,
                    ModDateTime = DateTime.Now,
                    CreateUser = "DB Initialization",
                    ModificationUser = "DB Initialization",
                    LoginScreenBackgroundImage = "/images/LoginBase.jpg",
                    ApplicationLogo = "/images/complete.png",
                    EmailPrefix = "MailGenerator",
                    ErrorEmailPrefix = "!!!Error",
                    AlertEmailPrefix = "!!!Alert",
                    ErrorEMailMessage = "<p>Hello,</p><p>An error has been raised during the process:</p><p>{0}</p><p>Your IT Team</p>",
                    AdminEmails = "[]"
                };
                await _context.AddAsync(Parameters);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }

        }

        private async Task DefaultServiceStatus()
        {
            try
            {
                ServicesStatus ServiceStat = new()
                {
                    CleanerService = false,
                    ReportService = false,
                    EmailService = false,
                    CreateDateTime = DateTime.Now,
                    ModDateTime = DateTime.Now,
                    CreateUser = "DB Initialization",
                    ModificationUser = "DB Initialization"
                };
                await _context.AddAsync(ServiceStat);
                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
