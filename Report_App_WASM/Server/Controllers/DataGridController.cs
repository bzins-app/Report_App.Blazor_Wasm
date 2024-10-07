using AutoMapper.QueryableExtensions;
using Community.OData.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
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
    public IQueryable<ApplicationLogSystem> GetSystemLogs()
    {
        return _context.ApplicationLogSystem.OrderByDescending(a => a.Id).AsNoTracking();
    }


    [HttpPost("odata/ExtractLogs")]
    public async Task<FileResult?> ExtractLogsAsync([FromBody] ODataExtractPayload values)
    {
        if (values.FunctionName == "EmailLogs") return await GetExtractFile(GetEmailLogs(), values);

        if (values.FunctionName == "QueryExecutionLogs") return await GetExtractFile(GetQueryExecutionLogs(), values);

        if (values.FunctionName == "ReportResultLogs") return await GetExtractFile(GetReportResultLogs(), values);

        if (values.FunctionName == "TaskLogs") return await GetExtractFile(GetTaskLogs(), values);

        if (values.FunctionName == "AuditTrail") return await GetExtractFile(GetAuditTrail(), values);

        if (values.FunctionName == "QueriesLogs") return await GetExtractFile(GetQueriesLogs(), values);

        return await GetExtractFile(GetSystemLogs(), values);
    }

    private async Task<FileResult> GetExtractFile<T>(IQueryable<T> source, ODataExtractPayload values) where T : class
    {
        var q = source.OData();
        if (!string.IsNullOrEmpty(values.FilterValues)) q = q.Filter(values.FilterValues.Replace("$filter=", ""));
        if (!string.IsNullOrEmpty(values.SortValues)) q = q.OrderBy(values.SortValues.Replace("$orderby=", ""));
        var finalQ = q.ToOriginalQuery();

        try
        {
            _logger.LogInformation("Grid extraction: Start " + values.FunctionName, values.FunctionName);
            var items = await finalQ.AsQueryable().Take(values.MaxResult).ToListAsync();
            var fileName = values.FileName + " " + DateTime.Now.ToString("yyyyMMdd_HH_mm_ss") + ".xlsx";

            var file = CreateFile.ExcelFromCollection(fileName, values.TabName, items);
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
    public IQueryable<ApplicationLogEmailSender> GetEmailLogs()
    {
        return _context.ApplicationLogEmailSender.OrderByDescending(a => a.Id).AsNoTracking();
    }

    [EnableQuery(EnsureStableOrdering = false)]
    [HttpGet("odata/QueriesLogs")]
    public IQueryable<ApplicationLogAdHocQueries> GetQueriesLogs()
    {
        return _context.ApplicationLogAdHocQueries.OrderByDescending(a => a.Id).AsNoTracking();
    }

    [EnableQuery(EnsureStableOrdering = false)]
    [HttpGet("odata/QueryExecutionLogs")]
    public IQueryable<ApplicationLogQueryExecutionDto> GetQueryExecutionLogs()
    {
        return _context.ApplicationLogQueryExecution
            .ProjectTo<ApplicationLogQueryExecutionDto>(_mapper.ConfigurationProvider).OrderByDescending(a => a.Id)
            .AsQueryable();
    }

    [EnableQuery(EnsureStableOrdering = false)]
    [HttpGet("odata/ReportResultLogs")]
    public IQueryable<ApplicationLogReportResult> GetReportResultLogs()
    {
        return _context.ApplicationLogReportResult.OrderByDescending(a => a.Id).AsNoTracking();
    }

    [EnableQuery(EnsureStableOrdering = false)]
    [HttpGet("odata/TaskLogs")]
    public IQueryable<ApplicationLogTask> GetTaskLogs()
    {
        return _context.ApplicationLogTask.OrderByDescending(a => a.Id).AsNoTracking();
    }

    [EnableQuery(EnsureStableOrdering = false)]
    [HttpGet("odata/AuditTrail")]
    public IQueryable<ApplicationAuditTrail> GetAuditTrail()
    {
        return _context.ApplicationAuditTrail.OrderByDescending(a => a.Id).AsNoTracking();
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
    public IQueryable<SftpConfiguration?> GetSftp()
    {
        return _context.SftpConfiguration;
    }

    [EnableQuery]
    [HttpGet("odata/DepositPath")]
    public IQueryable<FileDepositPathConfigurationDto> GetDepositPath()
    {
        return _context.FileDepositPathConfiguration
            .Select(a => new FileDepositPathConfigurationDto
            {
                ConfigurationName = a.ConfigurationName,
                CreateDateTime = a.CreateDateTime, ModDateTime = a.ModDateTime, CreateUser = a.CreateUser,
                FileDepositPathConfigurationId = a.FileDepositPathConfigurationId,
                FilePath = a.FilePath, ModificationUser = a.ModificationUser,
                SftpConfigurationId = a.SftpConfiguration == null ? 0 : a.SftpConfiguration.SftpConfigurationId,
                TryToCreateFolder = a.TryToCreateFolder, UseSftpProtocol = a.UseSftpProtocol,
                IsReachable = a.IsReachable
            }).AsQueryable();
    }

    [EnableQuery]
    [HttpGet("odata/Activities")]
    public IQueryable<Activity> GetActivities()
    {
        return _context.Activity.Where(a => a.ActivityType == ActivityType.SourceDb)
            .Include(a => a.ActivityDbConnections).AsNoTracking();
    }

    [EnableQuery]
    [HttpGet("odata/DataTransfers")]
    public IQueryable<Activity> GetDataTransfers()
    {
        return _context.Activity.Where(a => a.ActivityType == ActivityType.TargetDb)
            .Include(a => a.ActivityDbConnections).AsNoTracking();
    }

    [EnableQuery(EnsureStableOrdering = false)]
    [HttpGet("odata/TaskHeader")]
    public IQueryable<TaskHeader> GetTaskHeader()
    {
        return _context.TaskHeader.OrderByDescending(a => a.TaskHeaderId).AsNoTracking();
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
    public IQueryable<QueryStore> GetQueries()
    {
        return _context.QueryStore.OrderByDescending(a => a.Id);
    }
}