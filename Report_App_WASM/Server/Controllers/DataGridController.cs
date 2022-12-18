using AutoMapper;
using Community.OData.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Shared.ApiResponse;
using Report_App_WASM.Server.Utils;
using Microsoft.AspNetCore.OData.Formatter;
using Report_App_WASM.Shared;

namespace Report_App_WASM.Server.Controllers
{

    [ApiExplorerSettings(IgnoreApi =true)]
    public class DataGridController : ODataController, IDisposable
    {
        private readonly ILogger<DataGridController> _logger;
        private readonly ApplicationDbContext _context;
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
        public async Task<FileResult?> ExtractLogsAsync([FromBody] ODataExtractPayload Values)
        {
            if (Values.FunctionName == "EmailLogs")
            {
                return await GetExtractFile(GetEmailLogs(), Values);
            }
            else
            if (Values.FunctionName == "QueryExecutionLogs")
            {
                return await GetExtractFile(GetQueryExecutionLogs(), Values);
            }
            else
            if (Values.FunctionName == "ReportResultLogs")
            {
                return await GetExtractFile(GetReportResultLogs(), Values);
            }
            else
            if (Values.FunctionName == "TaskLogs")
            {
                return await GetExtractFile(GetTaskLogs(), Values);
            }
            else
            if (Values.FunctionName == "AuditTrail")
            {
                return await GetExtractFile(GetAuditTrail(), Values);
            }
            else
            {
                return await GetExtractFile(GetSystemLogs(), Values);
            }
        }

        private async Task<FileResult> GetExtractFile<T>(IQueryable<T> source, ODataExtractPayload Values) where T : class
        {
            var q = source.OData();
            if (!string.IsNullOrEmpty(Values.FilterValues))
            {
                q = q.Filter(Values.FilterValues);
            }
            if (!string.IsNullOrEmpty(Values.SortValues))
            {
                q = q.OrderBy(Values.SortValues);
            }
            var FinalQ = q.ToOriginalQuery();

            try
            {
                _logger.LogInformation("Grid extraction: Start ", Values.FunctionName);
                var items = await FinalQ.AsQueryable().Take(Values.MaxResult).ToListAsync();
                var fileName = Values.FileName + " " + DateTime.Now.ToString("yyyyMMdd_HH_mm_ss") + ".xlsx";

                var file = CreateFile.ExcelFromCollection(fileName, Values.TabName, items);
                _logger.LogInformation($"Grid extraction: End", $" {fileName} {items.Count} lines");
                return File(file.FileContents, contentType: file.ContentType, file.FileDownloadName);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null;
            }
        }


        [EnableQuery(EnsureStableOrdering = false)]
        [HttpGet("odata/EmailLogs")]
        public IQueryable<ApplicationLogEmailSender> GetEmailLogs()
        {
            return _context.ApplicationLogEmailSender.OrderByDescending(a=>a.Id).AsNoTracking();
        }

        [EnableQuery(EnsureStableOrdering = false)]
        [HttpGet("odata/QueryExecutionLogs")]
        public IQueryable<ApplicationLogQueryExecution> GetQueryExecutionLogs()
        {
            return _context.ApplicationLogQueryExecution.OrderByDescending(a=>a.Id).AsNoTracking();
        }

        [EnableQuery(EnsureStableOrdering = false)]
        [Route("odata/ReportResultLogs")]
        public IQueryable<ApplicationLogReportResult> GetReportResultLogs()
        {
            return _context.ApplicationLogReportResult.OrderByDescending(a=>a.Id).AsNoTracking();
        }

        [EnableQuery(EnsureStableOrdering = false)]
        [HttpGet("odata/TaskLogs")]
        public IQueryable<ApplicationLogTask> GetTaskLogs()
        {
            return _context.ApplicationLogTask.OrderByDescending(a=>a.Id).AsNoTracking();
        }

        [EnableQuery(EnsureStableOrdering = false)]
        [HttpGet("odata/AuditTrail")]
        public IQueryable<ApplicationAuditTrail> GetAuditTrail()
        {
            return _context.ApplicationAuditTrail.OrderByDescending(a=>a.Id).AsNoTracking();
        }

        [EnableQuery(EnsureStableOrdering = false)]
        [HttpGet("odata/SMTP")]
        public IQueryable<SMTPConfiguration> GetSMTP()
        {
            return _context.SMTPConfiguration.OrderByDescending(a => a.Id).AsNoTracking();
        }

        [EnableQuery()]
        [HttpGet("odata/LDAP")]
        public IQueryable<LDAPConfiguration> GetLDAP()
        {
            return _context.LDAPConfiguration.AsNoTracking();
        }

        [EnableQuery()]
        [HttpGet("odata/SFTP")]
        public IQueryable<SFTPConfiguration> GetSFTP()
        {
            return _context.SFTPConfiguration.AsNoTracking();
        }

        [EnableQuery()]
        [HttpGet("odata/DepositPath")]
        public IQueryable<FileDepositPathConfiguration> GetDepositPath()
        {
            return _context.FileDepositPathConfiguration.AsNoTracking();
        }

        [EnableQuery()]
        [HttpGet("odata/Activities")]
        public IQueryable<Activity> GetActivities()
        {
            return _context.Activity.Include(a=>a.ActivityDbConnections).AsNoTracking();
        }

        [EnableQuery(EnsureStableOrdering = false)]
        [HttpGet("odata/TaskHeader")]
        public IQueryable<TaskHeader> GetTaskHeader()
        {
            return _context.TaskHeader.OrderByDescending(a => a.TaskHeaderId).AsNoTracking();
        }

        [EnableQuery(EnsureStableOrdering = false)]
        [HttpGet("odata/Report")]
        public IQueryable<TaskHeader> GetTaskHeaderReport()
        {
            return _context.TaskHeader.Where(a=>a.Type==TaskType.Report).OrderByDescending(a => a.TaskHeaderId).AsNoTracking();
        }

        [EnableQuery(EnsureStableOrdering = false)]
        [HttpGet("odata/Report({activityId})")]
        public IQueryable<TaskHeader> GetTaskHeaderReport(int activityId)
        {
            return _context.TaskHeader.Where(a => a.Type == TaskType.Report&&a.Activity.ActivityId==activityId).OrderByDescending(a => a.TaskHeaderId).AsNoTracking();
        }

        [EnableQuery(EnsureStableOrdering = false)]
        [HttpGet("odata/Alert")]
        public IQueryable<TaskHeader> GetTaskHeaderAlert()
        {
            return _context.TaskHeader.Where(a => a.Type == TaskType.Alert).OrderByDescending(a => a.TaskHeaderId).AsNoTracking();
        }

        [EnableQuery(EnsureStableOrdering = false)]
        [HttpGet("odata/DataTransfer")]
        public IQueryable<TaskHeader> GetTaskHeaderDataTransfer()
        {
            return _context.TaskHeader.Where(a => a.Type == TaskType.DataTransfer).OrderByDescending(a => a.TaskHeaderId).AsNoTracking();
        }
    }
}
