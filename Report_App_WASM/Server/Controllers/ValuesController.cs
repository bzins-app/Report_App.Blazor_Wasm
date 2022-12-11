using AutoMapper;
using Community.OData.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Utils;
using Report_App_WASM.Shared.ApiResponse;

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
            Console.WriteLine("je vois vois vois voius vois ovis ovisoouivo iuvouivosuvouisujiovu uivusiuv ivusiusjviusi" + h.Count);
            return Ok("ok");
        }

        [HttpPost("ExtractLogs")]
        public async Task<FileResult> ExtractLogsAsync([FromBody] ODataExtractPayload Values)
        {
            Console.WriteLine("start");
            var q = _context.ApplicationLogSystem.OData(); ;
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


            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }


            var j = await FinalQ.Take(10000).ToListAsync();
            Console.WriteLine("result nbr of rows " + j.Count);
            Console.WriteLine("end");
            _logger.LogInformation("Grid extraction: Start " + Values.FunctionName);
            var items = await FinalQ.AsQueryable().Take(Values.MaxResult).ToListAsync();
            var fileName = Values.FileName + " " + DateTime.Now.ToString("yyyyMMdd_HH_mm_ss") + ".xlsx";

            var file = CreateFile.ExcelFromCollection(fileName, Values.TabName, items);
            //var downloadresult = await _blazorDownloadFileService.DownloadFile(fileName, file.FileContents, contentType: "application/octet-stream");
            _logger.LogInformation($"Grid extraction: End {fileName} {items.Count} lines");
            /* if (downloadresult.Succeeded)
             {
                 file = null;
                 items.Clear();
             }*/
            return File(file.FileContents, contentType: file.ContentType, file.FileDownloadName);

        }
    }
}
