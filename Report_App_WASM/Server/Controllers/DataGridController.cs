﻿using AutoMapper.QueryableExtensions;
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
    public IQueryable<SystemLog> GetSystemLogs()
    {
        return _context.SystemLog.OrderByDescending(a => a.Id).AsNoTracking();
    }

    [HttpPost("odata/ExtractLogs")]
    public async Task<FileResult?> ExtractLogsAsync([FromBody] ODataExtractPayload values)
    {
        return values.FunctionName switch
        {
            "EmailLogs" => await GetExtractFile(GetEmailLogs(), values),
            "QueryExecutionLogs" => await GetExtractFile(GetQueryExecutionLogs(), values),
            "ReportResultLogs" => await GetExtractFile(GetReportResultLogs(), values),
            "TaskLogs" => await GetExtractFile(GetTaskLogs(), values),
            "AuditTrail" => await GetExtractFile(GetAuditTrail(), values),
            "QueriesLogs" => await GetExtractFile(GetQueriesLogs(), values),
            _ => await GetExtractFile(GetSystemLogs(), values)
        };
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
    public IQueryable<SftpConfiguration?> GetSftp()
    {
        return _context.SftpConfiguration;
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
                SftpConfigurationId = a.SftpConfiguration == null ? 0 : a.SftpConfiguration.SftpConfigurationId,
                TryToCreateFolder = a.TryToCreateFolder,
                UseSftpProtocol = a.UseSftpProtocol,
                IsReachable = a.IsReachable
            }).AsQueryable();
    }

    [EnableQuery]
    [HttpGet("odata/Activities")]
    public IQueryable<DataProvider> GetActivities()
    {
        return _context.DataProvider.Where(a => a.ProviderType == ProviderType.SourceDatabase)
            .Include(a => a.DatabaseConnections).AsNoTracking();
    }

    [EnableQuery]
    [HttpGet("odata/DataTransfers")]
    public IQueryable<DataProvider> GetDataTransfers()
    {
        return _context.DataProvider.Where(a => a.ProviderType == ProviderType.TargetDatabase)
            .Include(a => a.DatabaseConnections).AsNoTracking();
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