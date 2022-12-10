using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.DTO;

namespace Report_App_WASM.Server.Controllers
{

    public class DataGridController : ODataController
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

        [EnableQuery()]
        [Route("odata/SystemLogs")]
        public IQueryable<ApplicationLogSystem> GetSystemLogs()
        {
            return  _context.ApplicationLogSystem.AsNoTracking();
        }

        [EnableQuery()]
        [Route("odata/SystemLogs{id}")]
        public IQueryable<ApplicationLogSystem> GetSystemLogs(ODataQueryOptions odataQueryOptions, [FromODataUri] int key)
        {
            Console.WriteLine("start");
            var q = _context.ApplicationLogSystem.AsNoTracking();
            q = (IQueryable<ApplicationLogSystem>)odataQueryOptions.ApplyTo(q,AllowedQueryOptions.Top|AllowedQueryOptions.Skip);

            var j=q.Take(10000).ToList();
            Console.WriteLine("result nbr of rows "+j.Count);
            Console.WriteLine("end");
            return q;
        }

        [EnableQuery()]
        [Route("odata/EmailLogs")]
        public IQueryable<ApplicationLogEmailSender> GetEmailLogs()
        {
            return _context.ApplicationLogEmailSender.AsNoTracking();
        }

        [EnableQuery()]
        [Route("odata/QueryExecutionLogs")]
        public IQueryable<ApplicationLogQueryExecution> GetQueryExecutionLogs()
        {
            return _context.ApplicationLogQueryExecution.AsNoTracking();
        }

        [EnableQuery()]
        [Route("odata/ReportResultLogs")]
        public IQueryable<ApplicationLogReportResult> GetReportResultLogs()
        {
            return _context.ApplicationLogReportResult.AsNoTracking();
        }

        [EnableQuery()]
        [Route("odata/TaskLogs")]
        public IQueryable<ApplicationLogTask> GetTaskLogs()
        {
            return _context.ApplicationLogTask.AsNoTracking();
        }

        [EnableQuery()]
        [Route("odata/AuditTrail")]
        public IQueryable<ApplicationAuditTrail> GetAuditTrail()
        {
            return _context.ApplicationAuditTrail.AsNoTracking();
        }
    }
}
