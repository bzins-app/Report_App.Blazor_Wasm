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

    public class ApplicationLogsController : ODataController
    {
        private readonly ILogger<ApplicationLogsController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ApplicationLogsController(ILogger<ApplicationLogsController> logger,
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

    }
}
