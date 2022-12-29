using BlazorDownloadFile;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Report_App_WASM.Client.Utils;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.ApiExchanges;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Report_App_WASM.Client.Services
{
    public class DataInteractionService
    {
        private readonly HttpClient _httpClient;
        private readonly IBlazorDownloadFileService _blazorDownloadFileService;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private const string CrudApi = ApiControllers.CrudDataApi;

        public DataInteractionService(HttpClient httpClient,
            IBlazorDownloadFileService blazorDownloadFileService, AuthenticationStateProvider authenticationStateProvider)
        {
            _httpClient = httpClient;
            _blazorDownloadFileService = blazorDownloadFileService;
            _authenticationStateProvider = authenticationStateProvider;
        }


        private async Task<string?> GetUserIdAsync()
        {
            return (await _authenticationStateProvider.GetAuthenticationStateAsync())?.User?.Identity?.Name;// FindFirst(ClaimTypes.NameIdentifier).Value;
        }


        public async Task<SubmitResult> PostValues<T>(T value, string controllerAction, string controller = CrudApi) where T : class
        {
            var uri = $"{controller}{controllerAction}";

            ApiCrudPayload<T> payload = new() { EntityValue = value, UserName = await GetUserIdAsync() };
            try
            {
                JsonSerializerOptions options = new()
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                };
                var response = await _httpClient.PostAsJsonAsync(uri, payload, options);
                if (response.StatusCode == HttpStatusCode.BadRequest) throw new Exception(await response.Content.ReadAsStringAsync());
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    return (await response.Content.ReadFromJsonAsync<SubmitResult>())!;
                }

                return new SubmitResult { Success = false };
            }
            catch (Exception ex)
            {
                return new SubmitResult { Success = false, Message = ex.Message };
            }
        }

        public async Task ExtractGridLogs(ODataExtractPayload values)
        {
            try
            {
                var url = "odata/ExtractLogs";
                var response = await _httpClient.PostAsJsonAsync(url, values);
                if (response.IsSuccessStatusCode)
                {
                    var downloadresult = await _blazorDownloadFileService.DownloadFile(values.FileName + " " + DateTime.Now.ToString("yyyyMMdd_HH_mm_ss") + ".xlsx", await response.Content.ReadAsByteArrayAsync(), contentType: "application/octet-stream");
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


        public async Task<List<T>> GetValues<T>(string controllerAction, string controller = CrudApi) where T : class
        {
            var uri = $"{controller}{controllerAction}";
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<T>>(uri);
                if (response != null)
                {
                    return response;
                }

                return new List<T>();
            }
            catch
            {
                return new List<T>();
            }
        }

        public async Task<T> GetUniqueValue<T>(T value, string controllerAction, string controller = CrudApi) where T : class
        {
            var uri = $"{controller}{controllerAction}";
            try
            {
                var response = await _httpClient.GetFromJsonAsync<T>(uri);
                if (response != null)
                {
                    return response;
                }

                return value;
            }
            catch
            {
                return value;
            }
        }

        public async Task<SubmitResult> PostFile(IBrowserFile file)
        {
            try
            {
                long maxFileSize = 1024 * 15;
                using var content = new MultipartFormDataContent();
                var fileContent =
                            new StreamContent(file.OpenReadStream(maxFileSize));

                fileContent.Headers.ContentType =
                    new MediaTypeHeaderValue(file.ContentType);
                content.Add(
                            content: fileContent,
                            name: "\"file\"",
                            fileName: file.Name);
                var response = await _httpClient.PostAsync($"{ApiControllers.FilesApi}Upload", content);
                return (await response.Content.ReadFromJsonAsync<SubmitResult>())!;
            }
            catch (Exception ex)
            {
                return new SubmitResult { Success = false, Message = ex.Message };
            }

        }
    }
}
