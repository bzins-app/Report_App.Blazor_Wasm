using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.DTO;

namespace Report_App_WASM.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ApplicationLogsController : Controller
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

        [HttpGet("SystemLogs")]
        public async Task<IEnumerable<ApplicationLogSystemDTO>> GetSystemLogs()
        {
            _logger.LogWarning("GridLoaded");
            return await _context.ApplicationLogSystem.AsNoTracking().ProjectTo<ApplicationLogSystemDTO>(_mapper.ConfigurationProvider).ToArrayAsync();
        }
    }
}
