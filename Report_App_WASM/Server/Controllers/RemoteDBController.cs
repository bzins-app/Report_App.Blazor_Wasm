using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Report_App_BlazorServ.Services.RemoteDb;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Utils.EncryptDecrypt;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.ApiExchanges;
using Report_App_WASM.Shared.DTO;
using Report_App_WASM.Shared.Extensions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Report_App_WASM.Server.Controllers
{
    [Route("api/[controller]/[Action]")]
    [ApiController]
    public class RemoteDBController : ControllerBase
    {
        private readonly IRemoteDbConnection _remoteDb;
        public RemoteDBController(IRemoteDbConnection remoteDb)
        {
            _remoteDb = remoteDb;
        }

        [HttpPost]
        public async Task<IActionResult> TestConnection(ApiCRUDPayload<ActivityDbConnectionDTO> value)
        {
            value.EntityValue.Password = EncryptDecrypt.DecryptString(value.EntityValue.Password!);
            var result = await _remoteDb.TestConnectionAsync(value.EntityValue);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDataTransferTable(string DataTransferTargetTableName)
        {
            await _remoteDb.DeleteTable(DataTransferTargetTableName);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RemoteDbGetValues(RemoteDataPayload values)
        {
            var data =await _remoteDb.RemoteDbToDatableAsync(values.values, values.Ct);
            return Ok(data.ToDictionnary());
        }
    }
}
