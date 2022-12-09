using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;

namespace Report_App_WASM.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        private readonly ILogger<ValuesController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ValuesController(ILogger<ValuesController> logger,
            ApplicationDbContext context, IMapper mapper)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
        }

        [EnableQuery()]
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var h = await _context.ApplicationLogSystem.AsQueryable().ToListAsync();
            Console.WriteLine("je vois vois vois voius vois ovis ovisoouivo iuvouivosuvouisujiovu uivusiuv ivusiusjviusi"+h.Count);
            return Ok("ok");
        }
    }
}
