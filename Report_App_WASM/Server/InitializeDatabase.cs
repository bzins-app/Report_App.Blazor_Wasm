using Microsoft.Extensions.Options;
using Report_App_WASM.Server.Utils.SettingsConfiguration;
using Report_App_WASM.Shared.DatabasesConnectionParameters;

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
            await UpdateRemoteConnectionValues();
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

    private async Task UpdateRemoteConnectionValues()
    {
        try
        {
            var con = await _context.ActivityDbConnection
                .Where(a => a.DbConnectionParameters == "[]" && a.ConnectionType == "SQL").ToListAsync();
            foreach (var t in con)
            {
                if (t.TypeDb == TypeDb.PostgreSql)
                {
                    PostgreSqlParameters PostgreParameters = new();
                    PostgreParameters.Server = t.ConnectionPath;
                    if (t.DbSchema != null) PostgreParameters.Database = t.DbSchema;
                    if (t.Port > 0)
                    {
                        PostgreParameters.Port = t.Port;
                    }
                    t.DbConnectionParameters = DatabaseConnectionParametersManager.SerializeToJson(PostgreParameters.SerializeMembersToJson(), TypeDb.PostgreSql);

                }
                else if (t.TypeDb == TypeDb.MariaDb)
                {
                    MariaDbParameters MariaParameters = new();
                    MariaParameters.Server = t.ConnectionPath;
                    if (t.DbSchema != null) MariaParameters.Database = t.DbSchema;
                    if (t.Port > 0)
                    {
                        MariaParameters.Port = t.Port;
                    }
                    t.DbConnectionParameters = DatabaseConnectionParametersManager.SerializeToJson(MariaParameters.SerializeMembersToJson(), TypeDb.MariaDb);

                }
                else if (t.TypeDb == TypeDb.MySql)
                {
                    MySqlParameters MyParameters = new();
                    MyParameters.Server = t.ConnectionPath;
                    if (t.DbSchema != null) MyParameters.Database = t.DbSchema;
                    if (t.Port > 0)
                    {
                        MyParameters.Port = t.Port;
                    }
                    t.DbConnectionParameters = DatabaseConnectionParametersManager.SerializeToJson(MyParameters.SerializeMembersToJson(), TypeDb.MySql);

                }
                else if (t.TypeDb == TypeDb.Db2)
                {
                    OleDbParameters oleDParameters = new();
                    if (t.ConnectionPath != null) oleDParameters.Server = t.ConnectionPath;
                    if (t.DbSchema != null) oleDParameters.Database = t.DbSchema;
                    oleDParameters.Provider = "DB2OLEDB.1";
                    t.DbConnectionParameters = DatabaseConnectionParametersManager.SerializeToJson(oleDParameters.SerializeMembersToJson(), TypeDb.OlebDb);
                    t.TypeDb = TypeDb.OlebDb;

                }
                else if (t.TypeDb == TypeDb.Oracle)
                {
                    OracleParameters OParameters = new();
                    if (t.ConnectionPath.Contains(":") || t.ConnectionPath.Contains("/"))
                    {
                        if (t.ConnectionPath.Contains(":"))
                        {
                            var _splitString = t.ConnectionPath.Split(":");
                            if (_splitString.Count() == 2)
                            {
                                OParameters.Server = _splitString[0];
                                if (_splitString[1].Contains("/"))
                                {
                                    var _splitString2 = _splitString[1].Split("/");
                                    if (_splitString2.Count() == 2)
                                    {
                                        OParameters.Port = Convert.ToInt32(_splitString2[0]);
                                        OParameters.ServiceName = _splitString2[1];
                                    }
                                }
                                else
                                {
                                    OParameters.Port = Convert.ToInt32(_splitString[1]);
                                }
                            }
                        }
                        else
                        {
                            var _splitString = t.ConnectionPath.Split("/");
                            if (_splitString.Count() == 2)
                            {
                                OParameters.Server = _splitString[0];
                                OParameters.ServiceName = _splitString[1];
                            }
                        }
                    }
                    else
                    {
                        OParameters.Server = t.ConnectionPath;
                    }

                    OParameters.UseDbSchema = t.UseDbSchema;
                    if (t.DbSchema != null) OParameters.Schema = t.DbSchema;
                    if (t.Port > 0)
                    {
                        OParameters.Port = t.Port;
                    }
                    t.DbConnectionParameters = DatabaseConnectionParametersManager.SerializeToJson(OParameters.SerializeMembersToJson(), TypeDb.Oracle);

                }
                else if (t.TypeDb == TypeDb.SqlServer)
                {
                    SqlServerParameters sqlServerParameters = new();
                    sqlServerParameters.Server = t.ConnectionPath;
                    if (t.IntentReadOnly) sqlServerParameters.ApplicationIntent = ApplicationIntent.ReadOnly;
                    if (t.DbSchema != null) sqlServerParameters.Database = t.DbSchema;
                    if (t.Port > 0)
                    {
                        sqlServerParameters.Port = t.Port;
                    }
                    sqlServerParameters.Encrypt = false;
                    t.DbConnectionParameters = DatabaseConnectionParametersManager.SerializeToJson(sqlServerParameters.SerializeMembersToJson(), TypeDb.SqlServer);

                }

                _context.Update(t);
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
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