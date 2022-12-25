using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Services.BackgroundWorker;
using Report_App_WASM.Server.Services.EmailSender;
using Report_App_WASM.Server.Services.FilesManagement;
using Report_App_WASM.Server.Services.RemoteDb;
using Report_App_WASM.Server.Utils;
using Report_App_WASM.Server.Utils.SettingsConfiguration;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureLogging((hostingContext, logging) =>
{
    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));

    logging.AddEntityFramework<ApplicationDbContext, ApplicationLogSystem>();

});

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

//builder.Services.AddIdentityServer()
//    .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

builder.Services.Configure<BaseUser>(builder.Configuration.GetSection("BaseUserDefaultOptions"));

var identityDefaultOptionsConfigurationSection = builder.Configuration.GetSection("IdentityDefaultOptions");
builder.Services.Configure<IdentityDefaultOptions>(identityDefaultOptionsConfigurationSection);

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IRemoteDbConnection, RemoteDbConnection>();
builder.Services.AddTransient<IBackgroundWorkers, BackgroundWorkers>();
builder.Services.AddTransient<LocalFilesService>();
builder.Services.AddTransient<InitializeDatabase>();

var identityDefaultOptions = identityDefaultOptionsConfigurationSection.Get<IdentityDefaultOptions>();
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = identityDefaultOptions!.PasswordRequireDigit;
    options.Password.RequiredLength = identityDefaultOptions.PasswordRequiredLength;
    options.Password.RequireNonAlphanumeric = identityDefaultOptions.PasswordRequireNonAlphanumeric;
    options.Password.RequireUppercase = identityDefaultOptions.PasswordRequireUppercase;
    options.Password.RequireLowercase = identityDefaultOptions.PasswordRequireLowercase;
    options.Password.RequiredUniqueChars = identityDefaultOptions.PasswordRequiredUniqueChars;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(identityDefaultOptions.LockoutDefaultLockoutTimeSpanInMinutes);
    options.Lockout.MaxFailedAccessAttempts = identityDefaultOptions.LockoutMaxFailedAccessAttempts;
    options.Lockout.AllowedForNewUsers = identityDefaultOptions.LockoutAllowedForNewUsers;

    // User settings
    options.User.RequireUniqueEmail = identityDefaultOptions.UserRequireUniqueEmail;

    // email confirmation require
    options.SignIn.RequireConfirmedEmail = identityDefaultOptions.SignInRequireConfirmedEmail;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});

builder.Services.AddControllersWithViews().AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles).AddOData(
options => options.AddRouteComponents(
"odata", OdataModels.GetEdmModel()).Select().Filter().OrderBy().Expand().Count().SetMaxTop(null));

builder.Services.AddRazorPages();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Hangfire services.
builder.Services.AddHangfire(configuration => configuration
    .UseDarkModeSupportForDashboard()
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new()
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

// Add the processing server as IHostedService
builder.Services.AddHangfireServer(options =>
{
    options.Queues = new[] { "default", "report", "cleaner", "alert", "datatransfer" };
});
builder.Services.AddDirectoryBrowser();


//enable response compresssion 
//builder.Services.AddResponseCompression(options =>
//{
//    options.Providers.Add<BrotliCompressionProvider>();
//    options.Providers.Add<GzipCompressionProvider>();
//    options.MimeTypes =
//        ResponseCompressionDefaults.MimeTypes.Concat(
//            new[] { "image/svg+xml" });
//});

//builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
//{
//    options.Level = CompressionLevel.Fastest;
//});

//builder.Services.Configure<GzipCompressionProviderOptions>(options =>
//{
//    options.Level = CompressionLevel.Fastest;
//});

var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new MappingProfile());
});

var mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

var app = builder.Build();
var env = builder.Environment;

using (var scope = app.Services.CreateScope())
{

    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        services.GetRequiredService<UserManager<ApplicationUser>>();
        services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var dbInit = services.GetRequiredService<InitializeDatabase>();

        dbInit.InitializeAsync().Wait();
        HashKey.Key = context.ApplicationUniqueKey.OrderBy(a => a.Id).Select(a => a.Id.ToString().Replace("-", "")).FirstOrDefault();
        var parameters = context.ApplicationParameters.FirstOrDefault();
        ApplicationConstants.ApplicationName = parameters?.ApplicationName!;
        ApplicationConstants.ApplicationLogo = parameters?.ApplicationLogo!;
        var ldapParameters = context.LdapConfiguration.Any(a => a.IsActivated);
        ApplicationConstants.LdapLogin = ldapParameters!;
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (!Directory.Exists(Path.Combine(env.ContentRootPath, "wwwroot/docsstorage")))
{
    Directory.CreateDirectory(Path.Combine(env.ContentRootPath, "wwwroot/docsstorage"));
}
if (!Directory.Exists(Path.Combine(env.ContentRootPath, "wwwroot/upload")))
{
    Directory.CreateDirectory(Path.Combine(env.ContentRootPath, "wwwroot/upload"));
}

//app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseRequestLocalization();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/Hangfire", new()
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.UseHangfireDashboard("/HangfireRead", new()
{
    IsReadOnlyFunc = context => true,
    Authorization = new[] { new HangfireAuthorizationFilterRead() }
});

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
});

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
