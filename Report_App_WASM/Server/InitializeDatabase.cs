﻿using Microsoft.Extensions.Options;
using Report_App_WASM.Server.Utils.SettingsConfiguration;

namespace Report_App_WASM.Server;

public class InitializeDatabase
{
    private readonly BaseUser _baseUser;
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

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
        await _context.Database.MigrateAsync();
        await _context.Database.EnsureCreatedAsync();

        // check for users
        if (_context.ApplicationUser.Any())
        {
            await InitAppRoles();
            return; //if user is not empty, DB has been seed
        }

        //init app with super admin user
        await CreateHashKey();
        await InitAppRoles();
        await CreateDefaultSuperAdmin();
        await ApplicationParameters();
        await DefaultServiceStatus();
    }

    private async Task CreateHashKey()
    {
        ApplicationUniqueKey key = new() { Id = Guid.NewGuid() };
        await _context.AddAsync(key);
        await _context.SaveChangesAsync();
    }

    private async Task InitAppRoles()
    {
        List<string> roles = new();
        string[] role =
        {
            "Admin",
            "Supervisor", "ApiAccess"
        };
        roles.AddRange(role);
        foreach (var t in roles)
            if (!await _roleManager.RoleExistsAsync(t).ConfigureAwait(true))
                await _roleManager.CreateAsync(new IdentityRole<Guid>(t)).ConfigureAwait(true);
    }

    private async Task CreateDefaultSuperAdmin()
    {
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
                ModDateTime = DateTime.Now,
                CreateDateTime = DateTime.Now
            }
            , _baseUser.Password!);

        //loop all the roles and then fill to SuperAdmin so he become powerfull
        var selectedUser = await _userManager.FindByEmailAsync(_baseUser.Email!).ConfigureAwait(true);
        if (selectedUser != null)
        {
            var roles = _roleManager.Roles.Select(a => a.Name).ToList();
            await _userManager.AddToRolesAsync(selectedUser, roles!).ConfigureAwait(true);
        }
    }

    private async Task ApplicationParameters()
    {
        ApplicationParameters parameters = new()
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
            ErrorEMailMessage =
                "<p>Hello,</p><p>An error has been raised during the process:</p><p>{0}</p><p>Your IT Team</p>",
            AdminEmails = "[]",
            ActivateAdHocQueriesModule = true,
            ActivateTaskSchedulerModule = true
        };
        await _context.AddAsync(parameters);
        await _context.SaveChangesAsync();
    }

    private async Task DefaultServiceStatus()
    {
        ServicesStatus serviceStat = new()
        {
            CleanerService = false,
            ReportService = false,
            EmailService = false,
            CreateDateTime = DateTime.Now,
            ModDateTime = DateTime.Now,
            CreateUser = "DB Initialization",
            ModificationUser = "DB Initialization"
        };
        await _context.AddAsync(serviceStat);
        await _context.SaveChangesAsync();
    }
}