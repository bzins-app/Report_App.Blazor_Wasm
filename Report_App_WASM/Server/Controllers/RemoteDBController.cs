using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Services.RemoteDb;
using Report_App_WASM.Server.Utils;
using Report_App_WASM.Server.Utils.EncryptDecrypt;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.ApiExchanges;
using Report_App_WASM.Shared.DTO;
using Report_App_WASM.Shared.Extensions;
using Report_App_WASM.Shared.RemoteQueryParameters;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Report_App_WASM.Server.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize]
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class RemoteDbController : ControllerBase, IDisposable
    {
        private readonly IRemoteDbConnection _remoteDb;
        private readonly ILogger<RemoteDbController> _logger;
        private readonly ApplicationDbContext _context;
        public RemoteDbController(IRemoteDbConnection remoteDb, ILogger<RemoteDbController> logger, ApplicationDbContext context)
        {
            _remoteDb = remoteDb;
            _logger = logger;
            _context = context;
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        [HttpPost]
        public async Task<IActionResult> TestConnection(ApiCrudPayload<ActivityDbConnectionDto?> value)
        {
            value.EntityValue!.Password = EncryptDecrypt.DecryptString(value.EntityValue.Password!);
            var result = await _remoteDb.TestConnectionAsync(value.EntityValue);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDataTransferTable(string? dataTransferTargetTableName)
        {
            await _remoteDb.DeleteTable(dataTransferTargetTableName);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RemoteDbGetValues(RemoteDataPayload values, CancellationToken ct)
        {
            try
            {
                var data = await _remoteDb.RemoteDbToDatableAsync(values.Values!, ct);
                var result = new SubmitResultRemoteData { Success = true, Value = data.ToDictionnary() };
                return Ok(result);
            }
            catch (Exception e)
            {
                var result = new SubmitResultRemoteData { Success = false, Message = e.Message, Value = new() };
                return Ok(result);
            }
        }

        [HttpPost]
        public async Task<FileResult?> RemoteDbExtractValuesAsync(RemoteDbCommandParameters values, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("Grid extraction: Start " + values.FileName, values.FileName);
                var items = await _remoteDb.RemoteDbToDatableAsync(values!, ct);
                var fileName = values.FileName + " " + DateTime.Now.ToString("yyyyMMdd_HH_mm_ss") + ".xlsx";

                var file = CreateFile.ExcelFromDatable(fileName, new ExcelCreationDatatable(values.FileName, new(), items));
                _logger.LogInformation($"Grid extraction: End {fileName} {items.Rows.Count} lines", $" {fileName} {items.Rows.Count} lines");
                return File(file.FileContents, contentType: file.ContentType, file.FileDownloadName);

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null!;
            }
        }

        [HttpGet]
        public async Task<DbTablesColList> GetTablesListAsync(int activityId, CancellationToken ct)
        {
            DbTablesColList listTables = new();
            var script = await _remoteDb.GetAllTablesScript(activityId);
            if (!string.IsNullOrEmpty(script))
            {
                RemoteDbCommandParameters parameters = new RemoteDbCommandParameters
                { ActivityId = activityId, QueryToRun = script, Test = true };
                var data = await _remoteDb.RemoteDbToDatableAsync(parameters, ct);
                var description = await _context.ActivityDbConnection
                    .Where(a => a.Activity.ActivityId == activityId).AsNoTracking().Select(a => new { tableDesc = a.UseTablesDescriptions, ConectId = a.Id })
                    .FirstOrDefaultAsync(cancellationToken: ct);

                var tables = data.AsEnumerable()
                    .Select(r => r.Field<string>(0))
                    .ToList();
                if (tables != null)
                {

                    if (description.tableDesc)
                    {
                        var Prework = await _context.DbTableDescriptions.Where(a => a.ActivityDbConnection.Id == description.ConectId).AsNoTracking().Select(a => new { a.TableName, a.TableDescription }).Distinct().ToListAsync(cancellationToken: ct);
                        listTables.Values = (from a in tables
                                             join b in Prework on a equals b.TableName into c
                                             from d in c.DefaultIfEmpty()
                                             select new DescriptionValues { Name = a, Description = d?.TableDescription }).ToList();
                        listTables.HasDescription = true;
                    }
                    else
                    {
                        foreach (var table in tables.Distinct())
                        {
                            listTables.Values.Add(new DescriptionValues { Name = table! });
                        }
                    }
                }
            }

            return listTables;
        }

        [HttpGet]
        public async Task<DbTablesColList> GetColumnListAsync(int activityId, string table, CancellationToken ct)
        {
            DbTablesColList listCols = new();
            var script = await _remoteDb.GetTableColumnInfoScript(activityId, table);
            if (!string.IsNullOrEmpty(script))
            {
                RemoteDbCommandParameters parameters = new RemoteDbCommandParameters
                { ActivityId = activityId, QueryToRun = script, Test = true };
                var data = await _remoteDb.RemoteDbToDatableAsync(parameters, ct);
                var description = await _context.ActivityDbConnection
                    .Where(a => a.Activity.ActivityId == activityId).AsNoTracking().Select(a => new { tableDesc = a.UseTablesDescriptions, ConectId = a.Id })
                    .FirstOrDefaultAsync(cancellationToken: ct);

                if (data.Rows.Count > 0)
                {
                    var cols = data.AsEnumerable()
                    .Select(r => r.Field<string>(0))
                    .ToList();
                    if (cols != null)
                    {
                        if (description.tableDesc&& await _context.DbTableDescriptions.Where(a => a.ActivityDbConnection.Id == description.ConectId && a.TableName == table).AnyAsync(cancellationToken: ct))
                        {
                            var desc = await _context.DbTableDescriptions.Where(a => a.ActivityDbConnection.Id == description.ConectId && a.TableName == table).AsNoTracking().Select(a => new { Name = a.ColumnName, Desciption = a.ColumnDescription ?? string.Empty }).ToListAsync();
                            if (desc != null)
                            {
                                listCols.HasDescription = true;
                                foreach (var col in cols.Distinct())
                                {
                                    listCols.Values.Add(new DescriptionValues { Name = col!, Description = desc.Where(a => a.Name == col!).Select(a => a.Desciption).FirstOrDefault() ?? string.Empty });
                                }
                            }
                            else
                            {
                                foreach (var col in cols.Distinct())
                                {
                                    listCols.Values.Add(new DescriptionValues { Name = col });
                                    listCols.HasDescription = false;
                                }
                            }

                        }
                        else
                        {
                            foreach (var col in cols.Distinct())
                            {
                                listCols.Values.Add(new DescriptionValues { Name = col });
                            }
                        }

                    }
                }
            }


            return listCols;
        }

    }
}
