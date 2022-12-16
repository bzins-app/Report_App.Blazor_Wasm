using BlazorDownloadFile;
using System.Net.Http.Json;
using Report_App_WASM.Shared.ApiResponse;
using Report_App_WASM.Shared;
using Microsoft.AspNetCore.Components.Authorization;

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


        public async Task<SubmitResult> PostValues<T>(T value, string ControllerAction, string controller=CrudAPI) where T : class
        {
            string uri = $"{controller}{ControllerAction}";

            ApiCRUDPayload<T> payload = new() { EntityValue = value, UserName = await GetUserIdAsync() };
            try
            {
                var response = await _httpClient.PostAsJsonAsync(uri, payload);
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest) throw new Exception(await response.Content.ReadAsStringAsync());
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {

                    return new SubmitResult { Success = true };
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


        public async Task<List<T>> GetValues<T>(string ControllerAction, string controller = CrudAPI) where T : class
        {
            string uri = $"{controller}{ControllerAction}";
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<T>>(uri);
                if (response != null)
                {
                    return response;
                }
                else
                {
                    return new List<T>();
                }
            }
            catch
            {
                return new List<T>();
            }
        }


    }
}
