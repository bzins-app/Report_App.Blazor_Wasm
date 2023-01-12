using BlazorDownloadFile;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Report_App_WASM.Client.Utils;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.ApiExchanges;
using Report_App_WASM.Shared.RemoteQueryParameters;
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

        public event Action<bool>? NotifyNotConnected;
        private bool _alreadyNotified;


        private async Task SendNotification()
        {
            if (!_alreadyNotified)
            {
                _alreadyNotified = true;
                NotifyNotConnected.Invoke(true);
            }
            await Task.CompletedTask;
        }

        public async Task<SubmitResult> PostValues<T>(T value, string controllerAction, string controller = CrudApi, CancellationToken ct = default) where T : class?
        {
            var uri = $"{controller}{controllerAction}";

            ApiCrudPayload<T> payload = new() { EntityValue = value, UserName = await GetUserIdAsync() };
            try
            {
                JsonSerializerOptions options = new()
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                };
                var response = await _httpClient.PostAsJsonAsync(uri, payload, options, cancellationToken: ct);
                if (response.StatusCode == HttpStatusCode.BadRequest) throw new Exception(await response.Content.ReadAsStringAsync(ct));
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                    response.StatusCode == HttpStatusCode.RequestTimeout)
                {
                    await SendNotification();
                }
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    _alreadyNotified = false;
                    return (await response.Content.ReadFromJsonAsync<SubmitResult>(cancellationToken: ct))!;
                }

                return new SubmitResult { Success = false };
            }
            catch (Exception ex)
            {
                return new SubmitResult { Success = false, Message = ex.Message };
            }
        }

        public async Task<SubmitResult> PostValuesLogJob<T>(T value, string controllerAction, string controller = CrudApi, CancellationToken ct = default) where T : class?
        {
            var uri = $"{controller}{controllerAction}";
            using HttpClient _httpClientLong = new HttpClient();
            _httpClientLong.Timeout = TimeSpan.FromMinutes(10);
            _httpClientLong.BaseAddress = _httpClient.BaseAddress;

            ApiCrudPayload<T> payload = new() { EntityValue = value, UserName = await GetUserIdAsync() };
            try
            {
                JsonSerializerOptions options = new()
                {
                    ReferenceHandler = ReferenceHandler.IgnoreCycles
                };
                var response = await _httpClientLong.PostAsJsonAsync(uri, payload, options, cancellationToken: ct);
                if (response.StatusCode == HttpStatusCode.BadRequest) throw new Exception(await response.Content.ReadAsStringAsync(ct));
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                    response.StatusCode == HttpStatusCode.RequestTimeout)
                {
                    await SendNotification();
                }
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    _alreadyNotified = false;
                    return (await response.Content.ReadFromJsonAsync<SubmitResult>(cancellationToken: ct))!;
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
                using HttpClient _httpClientLong = new HttpClient();
                _httpClientLong.Timeout = TimeSpan.FromMinutes(10);
                _httpClientLong.BaseAddress = _httpClient.BaseAddress;
                var response = await _httpClientLong.PostAsJsonAsync(url, values);
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                    response.StatusCode == HttpStatusCode.RequestTimeout)
                {
                    await SendNotification();
                }
                if (response.IsSuccessStatusCode)
                {
                    _alreadyNotified = false;
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


        public async Task ExtractAdHocQuery(RemoteDbCommandParameters values, CancellationToken ct)
        {
            try
            {
                var url = $"{ApiControllers.RemoteDbApi}RemoteDbExtractValues";
                using HttpClient _httpClientLong = new HttpClient();
                _httpClientLong.Timeout = TimeSpan.FromMinutes(10);
                _httpClientLong.BaseAddress = _httpClient.BaseAddress;
                var response = await _httpClientLong.PostAsJsonAsync(url, values);
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                    response.StatusCode == HttpStatusCode.RequestTimeout)
                {
                    await SendNotification();
                }
                if (response.IsSuccessStatusCode)
                {
                    _alreadyNotified = false;
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
                var response = await _httpClient.GetAsync(uri);
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                    response.StatusCode == HttpStatusCode.RequestTimeout)
                {
                    await SendNotification();
                }
                if (response.IsSuccessStatusCode)
                {
                    return (await response.Content.ReadFromJsonAsync<List<T>>())!;
                }

                return new List<T>();
            }
            catch
            {
                return new List<T>();
            }
        }

        public async Task<T> GetUniqueValue<T>(T value, string controllerAction, string controller = CrudApi) where T : class?
        {
            var uri = $"{controller}{controllerAction}";
            try
            {
                var response = await _httpClient.GetAsync(uri);
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                    response.StatusCode == HttpStatusCode.RequestTimeout)
                {
                    await SendNotification();
                }
                if (response.IsSuccessStatusCode)
                {
                    return (await response.Content.ReadFromJsonAsync<T>())!;
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
                long maxFileSize = 1024 * 1024 * 20;
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
