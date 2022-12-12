using BlazorDownloadFile;
using static System.Net.WebRequestMethods;
using System;
using System.Net.Http.Json;
using Report_App_WASM.Shared.ApiResponse;
using Report_App_WASM.Shared.DTO;
using Report_App_WASM.Shared;
using static MudBlazor.Colors;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Report_App_WASM.Client.Services
{
    public class DataEntityService
    {
        private readonly HttpClient _httpClient;
        private readonly IBlazorDownloadFileService BlazorDownloadFileService;
        private readonly AuthenticationStateProvider _AuthenticationStateProvider;
        private const string CrudAPI = "api/DataCrud/";

        public DataEntityService(HttpClient httpClient,
            IBlazorDownloadFileService blazorDownloadFileService, AuthenticationStateProvider AuthenticationStateProvider)
        {
            _httpClient = httpClient;
            BlazorDownloadFileService = blazorDownloadFileService;
            _AuthenticationStateProvider = AuthenticationStateProvider;
        }


        private async Task<string> GetUserIdAsync()
        {
            return (await _AuthenticationStateProvider.GetAuthenticationStateAsync())?.User?.Identity?.Name;// FindFirst(ClaimTypes.NameIdentifier).Value;
        }

        private async Task<SubmitResult> PostValues<T>(string uri, T value) where T : class
        {
            ApiCRUDPayload<T> payload = new ApiCRUDPayload<T> { EntityValue = value, UserName = await GetUserIdAsync() };
            try
            {
                var response = await _httpClient.PostAsJsonAsync(uri, payload);
                if (response.IsSuccessStatusCode)
                {

                    return new SubmitResult { Success = true};
                }
                else
                {
                    return new SubmitResult { Success = false };
                }
            }
            catch (Exception ex)
            {
                return new SubmitResult { Success = false, Message = ex.Message };
            }
        }

        public async Task<SubmitResult> HandlePostValues<T>(T value, string ControllerAction, string Uri=CrudAPI) where T : class
        {
            string uri = $"{Uri}{ControllerAction}";
            return await PostValues(uri, value);
        }

        public async Task ExtractGridLogs(ODataExtractPayload Values)
        {
            try
            {              
                string url = "odata/ExtractLogs";
                var response = await _httpClient.PostAsJsonAsync(url, Values);
                if(response.IsSuccessStatusCode)
                {
                    var downloadresult = await BlazorDownloadFileService.DownloadFile(Values.FileName + " " + DateTime.Now.ToString("yyyyMMdd_HH_mm_ss") + ".xlsx", await response.Content.ReadAsByteArrayAsync(), contentType: "application/octet-stream");
                    if (downloadresult.Succeeded)
                    {
                        response.Dispose();
                    }
                }
            }
            catch
            {
                // Unfortunately this HTTP API returns a 404 if there were no results, so we have to handle that separately
                //return null;
            }
        }

        public async Task<List<ApplicationLogTaskDetailsDTO>> GetLogTaskDetailsAsync(int LogTaskHeader)
        {
            string uri = $"{CrudAPI}LogTaskDetails/{LogTaskHeader}";
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<ApplicationLogTaskDetailsDTO>>(uri);
                if (response != null)
                {
                    return response;
                }
                else
                {
                    return new List<ApplicationLogTaskDetailsDTO>();
                }
            }
            catch
            {
                return new List<ApplicationLogTaskDetailsDTO>();
            }
        }

        public async Task<List<ActivityDbConnectionDTO>> GetActivityDbConnection(int ActivityId)
        {
            string uri = $"{CrudAPI}ActivityDbConnection/{ActivityId}";
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<ActivityDbConnectionDTO>>(uri);
                if (response != null)
                {
                    return response;
                }
                else
                {
                    return new List<ActivityDbConnectionDTO>();
                }
            }
            catch
            {
                return new List<ActivityDbConnectionDTO>();
            }
        }

    }
}
