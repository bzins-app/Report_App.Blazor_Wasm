using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Report_App_WASM.Server.Services.RemoteDb;
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
    public class RemoteDbController : ControllerBase
    {
        private readonly IRemoteDbConnection _remoteDb;
        public RemoteDbController(IRemoteDbConnection remoteDb)
        {
            _remoteDb = remoteDb;
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
                var tables=data.AsEnumerable()
                    .Select(r => r.Field<string>(0))
                    .ToList();
                if (tables != null)
                {
                    foreach (var table in tables)
                    {
                        listTables.Values.Add(table!,"empty");
                    }
                }
            }

            listTables.HasDescription=false;
            return listTables;
        }

        [HttpGet]
        public async Task<DbTablesColList> GetColumnListAsync(int activityId, string column, CancellationToken ct)
        {
            DbTablesColList listCols = new();
            var script = await _remoteDb.GetTableColummnInfoScript(activityId, column);
            if (!string.IsNullOrEmpty(script))
            {
                RemoteDbCommandParameters parameters = new RemoteDbCommandParameters
                    { ActivityId = activityId, QueryToRun = script, Test = true };
                var data = await _remoteDb.RemoteDbToDatableAsync(parameters, ct);
                var tables = data.AsEnumerable()
                    .Select(r => r.Field<string>(0))
                    .ToList();
                if (tables != null)
                {
                    foreach (var table in tables)
                    {
                        listCols.Values.Add(table!, null!);
                    }
                }
            }

            listCols.HasDescription = false;
            return listCols;
        }
    }
}
