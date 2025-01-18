using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Report_App_WASM.Server.Utils.FIles;
using Report_App_WASM.Shared.DTO;

namespace Report_App_WASM.Server.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
public class DataGridController : ODataController, IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataGridController> _logger;
    private readonly IMapper _mapper;

    public DataGridController(ILogger<DataGridController> logger,
        ApplicationDbContext context, IMapper mapper)
    {
        _logger = logger;
        _context = context;
        _mapper = mapper;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    [EnableQuery(EnsureStableOrdering = false)]
    [HttpGet("odata/SystemLogs")]
    public IQueryable<SystemLog> GetSystemLogs()
    {
        return _context.SystemLog.OrderByDescending(a => a.Id).AsNoTracking();
    }


    [EnableQuery]
    [HttpGet("odata/ExtractSystemLogs")]
    public async Task<IActionResult> ExtractSystemLogs(ODataQueryOptions<SystemLog> queryOptions)
    {
        var logs = await queryOptions.ApplyTo(_context.SystemLog.OrderByDescending(a => a.Id).AsNoTracking())
            .Cast<SystemLog>().ToListAsync();
        return GetExtractFile(logs, "SystemLogs", "SystemLogs");
    }

    [EnableQuery]
    [HttpGet("odata/ExtractEmailLogs")]
    public async Task<IActionResult> ExtractEmailLogs(ODataQueryOptions<EmailLog> queryOptions)
    {
        var logs = await queryOptions.ApplyTo(_context.EmailLog.OrderByDescending(a => a.Id).AsNoTracking())
            .Cast<EmailLog>().ToListAsync();
        return GetExtractFile(logs, "EmailLogs", "EmailLogs");
    }

    [EnableQuery]
    [HttpGet("odata/ExtractReportGenerationLogs")]
    public async Task<IActionResult> ExtractEmailLogs(ODataQueryOptions<ReportGenerationLog> queryOptions)
    {
        var logs = await queryOptions.ApplyTo(_context.ReportGenerationLog.OrderByDescending(a => a.Id).AsNoTracking())
            .Cast<ReportGenerationLog>().ToListAsync();
        return GetExtractFile(logs, "ReportGenerationLogs", "ReportGenerationLogs");
    }

    [EnableQuery]
    [HttpGet("odata/ExtractTaskLogs")]
    public async Task<IActionResult> ExtractTaskLogs(ODataQueryOptions<TaskLog> queryOptions)
    {
        var systemLogs = await queryOptions.ApplyTo(_context.TaskLog.OrderByDescending(a => a.TaskLogId).AsNoTracking())
            .Cast<TaskLog>().ToListAsync();
        return GetExtractFile(systemLogs, "TaskLogs", "TaskLogs");
    }

    [EnableQuery]
    [HttpGet("odata/ExtractQueryExecutionLogs")]
    public async Task<IActionResult> ExtractQueryExecutionLogs(ODataQueryOptions<QueryExecutionLog> queryOptions)
    {
        var logs = await queryOptions.ApplyTo(_context.QueryExecutionLog.OrderByDescending(a => a.Id).AsNoTracking())
            .Cast<QueryExecutionLog>().ToListAsync();
        return GetExtractFile(logs, "QueryExecutionLogs", "QueryExecutionLogs");
    }

    [EnableQuery]
    [HttpGet("odata/ExtractAuditTrail")]
    public async Task<IActionResult> ExtractAuditTrail(ODataQueryOptions<AuditTrail> queryOptions)
    {
        var logs = await queryOptions.ApplyTo(_context.AuditTrail.OrderByDescending(a => a.Id).AsNoTracking())
            .Cast<AuditTrail>().ToListAsync();
        return GetExtractFile(logs, "AuditTrail", "AuditTrail");
    }

    [EnableQuery]
    [HttpGet("odata/ExtractQueriesLogs")]
    public async Task<IActionResult> ExtractQueriesLogs(ODataQueryOptions<AdHocQueryExecutionLog> queryOptions)
    {
        var logs = await queryOptions
            .ApplyTo(_context.AdHocQueryExecutionLog.OrderByDescending(a => a.Id).AsNoTracking())
            .Cast<AdHocQueryExecutionLog>().ToListAsync();
        return GetExtractFile(logs, "QueriesLogs", "QueriesLogs");
    }


    private FileResult GetExtractFile<T>(List<T> items, string fname, string tab = "data") where T : class
    {
        try
        {
            _logger.LogInformation("Grid extraction: Start " + fname, fname);
            var fileName = fname + " " + DateTime.Now.ToString("yyyyMMdd_HH_mm_ss") + ".xlsx";

            var file = CreateFile.ExcelFromCollection(fileName, tab, items);
            _logger.LogInformation($"Grid extraction: End {fileName} {items.Count} lines",
                $" {fileName} {items.Count} lines");
            return File(file.Content, file.ContentType, file.FileName);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null!;
        }
    }

    [EnableQuery(EnsureStableOrdering = false)]
    [HttpGet("odata/UsersRole")]
    public IQueryable<UsersPerRole> GetUserPerRole()
    {
        var baseUserId = _context.Users.Where(b => b.IsBaseUser).Select(a => a.Id).FirstOrDefault();
        return _context.UserRoles.Where(a => a.UserId != baseUserId).Select(a => new UsersPerRole
        {
            RoleName = _context.Roles.Where(b => b.Id == a.RoleId).Select(a => a.Name).FirstOrDefault(),
            UserName = _context.Users.Where(b => b.Id == a.UserId && !b.IsBaseUser).Select(a => a.UserName)
                .FirstOrDefault()
        }).OrderBy(a => a.RoleName).AsQueryable();
    }

    [EnableQuery(EnsureStableOrdering = false)]
    [HttpGet("odata/EmailLogs")]
    public IQueryable<EmailLog> GetEmailLogs()
    {
        return _context.EmailLog.OrderByDescending(a => a.Id).AsNoTracking();
    }

    [EnableQuery(EnsureStableOrdering = false)]
    [HttpGet("odata/QueriesLogs")]
    public IQueryable<AdHocQueryExecutionLog> GetQueriesLogs()
    {
        return _context.AdHocQueryExecutionLog.OrderByDescending(a => a.Id).AsNoTracking();
    }

    [EnableQuery(EnsureStableOrdering = false)]
    [HttpGet("odata/QueryExecutionLogs")]
    public IQueryable<QueryExecutionLogDto> GetQueryExecutionLogs()
    {
        return _context.QueryExecutionLog
            .ProjectTo<QueryExecutionLogDto>(_mapper.ConfigurationProvider).OrderByDescending(a => a.Id)
            .AsQueryable();
    }

    [EnableQuery(EnsureStableOrdering = false)]
    [HttpGet("odata/ReportResultLogs")]
    public IQueryable<ReportGenerationLog> GetReportResultLogs()
    {
        return _context.ReportGenerationLog.OrderByDescending(a => a.Id).AsNoTracking();
    }

    [EnableQuery(EnsureStableOrdering = false)]
    [HttpGet("odata/TaskLogs")]
    public IQueryable<TaskLog> GetTaskLogs()
    {
        return _context.TaskLog.OrderByDescending(a => a.TaskLogId).AsNoTracking();
    }

    [EnableQuery(EnsureStableOrdering = false)]
    [HttpGet("odata/AuditTrail")]
    public IQueryable<AuditTrail> GetAuditTrail()
    {
        return _context.AuditTrail.OrderByDescending(a => a.Id).AsNoTracking();
    }

    [EnableQuery(EnsureStableOrdering = false)]
    [HttpGet("odata/Smtp")]
    public IQueryable<SmtpConfiguration> GetSmtp()
    {
        return _context.SmtpConfiguration.OrderByDescending(a => a.Id).AsNoTracking();
    }

    [EnableQuery]
    [HttpGet("odata/Ldap")]
    public IQueryable<LdapConfiguration> GetLdap()
    {
        return _context.LdapConfiguration.AsNoTracking();
    }

    [EnableQuery]
    [HttpGet("odata/Sftp")]
    public IQueryable<FileStorageConfiguration?> GetSftp()
    {
        return _context.FileStorageConfiguration;
    }

    [EnableQuery]
    [HttpGet("odata/DepositPath")]
    public IQueryable<FileStorageLocationDto> GetDepositPath()
    {
        return _context.FileStorageLocation
            .Select(a => new FileStorageLocationDto
            {
                ConfigurationName = a.ConfigurationName,
                CreateDateTime = a.CreateDateTime,
                ModDateTime = a.ModDateTime,
                CreateUser = a.CreateUser,
                FileStorageLocationId = a.FileStorageLocationId,
                FilePath = a.FilePath,
                ModificationUser = a.ModificationUser,
                FileStorageConfigurationId = a.FileStorageConfiguration == null ? 0 : a.FileStorageConfiguration.FileStorageConfigurationId,
                TryToCreateFolder = a.TryToCreateFolder,
                UseFileStorageConfiguration = a.UseFileStorageConfiguration,
                IsReachable = a.IsReachable
            }).AsQueryable();
    }

    [EnableQuery]
    [HttpGet("odata/Activities")]
    public IQueryable<DataProvider> GetActivities()
    {
        return _context.DataProvider.Where(a => a.ProviderType == ProviderType.SourceDatabase)
            .Include(a => a.DatabaseConnection).AsNoTracking();
    }

    [EnableQuery]
    [HttpGet("odata/DataTransfers")]
    public IQueryable<DataProvider> GetDataTransfers()
    {
        return _context.DataProvider.Where(a => a.ProviderType == ProviderType.TargetDatabase)
            .Include(a => a.DatabaseConnection).AsNoTracking();
    }

    [EnableQuery(EnsureStableOrdering = false)]
    [HttpGet("odata/TaskHeader")]
    public IQueryable<ScheduledTask> GetTaskHeader()
    {
        return _context.ScheduledTask.OrderByDescending(a => a.ScheduledTaskId).AsNoTracking();
    }

    [EnableQuery(EnsureStableOrdering = false)]
    [HttpGet("odata/Users")]
    public IQueryable<ApplicationUserDto> GetUsers()
    {
        return _context.ApplicationUser.Where(a => !a.IsBaseUser)
            .ProjectTo<ApplicationUserDto>(_mapper.ConfigurationProvider).OrderByDescending(a => a.CreateDateTime)
            .AsQueryable();
    }

    [EnableQuery(EnsureStableOrdering = false)]
    [HttpGet("odata/Queries")]
    public IQueryable<StoredQuery> GetQueries()
    {
        return _context.StoredQuery.OrderByDescending(a => a.Id);
    }
}