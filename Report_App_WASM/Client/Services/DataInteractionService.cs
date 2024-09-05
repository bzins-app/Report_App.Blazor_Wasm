using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using BlazorDownloadFile;

namespace Report_App_WASM.Client.Services;

public class DataInteractionService
{
    private const string CrudApi = ApiControllers.CrudDataApi;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IBlazorDownloadFileService _blazorDownloadFileService;
    private readonly HttpClient _httpClient;
    private bool _alreadyNotified;

    public DataInteractionService(HttpClient httpClient,
        IBlazorDownloadFileService blazorDownloadFileService, AuthenticationStateProvider authenticationStateProvider)
    {
        _httpClient = httpClient;
        _blazorDownloadFileService = blazorDownloadFileService;
        _authenticationStateProvider = authenticationStateProvider;
    }


    private async Task<string?> GetUserIdAsync()
    {
        return (await _authenticationStateProvider.GetAuthenticationStateAsync())?.User?.Identity
            ?.Name; // FindFirst(ClaimTypes.NameIdentifier).Value;
    }

    public event Action<bool>? NotifyNotConnected;


    private async Task SendNotification()
    {
        if (!_alreadyNotified)
        {
            _alreadyNotified = true;
            NotifyNotConnected.Invoke(true);
        }

        await Task.CompletedTask;
    }

    public async Task<SubmitResult> PostValues<T>(T value, string controllerAction, string controller = CrudApi,
        CancellationToken ct = default) where T : class?
    {
        var uri = $"{controller}{controllerAction}";
        if (controllerAction != "RunManually"|| controllerAction != "TestConnection")
        {
            var role = (await _authenticationStateProvider.GetAuthenticationStateAsync())?.User?.IsInRole("Demo");
            if (role.HasValue&&role.Value)
            {
                return new SubmitResult { Success = false, Message = "Demo mode" };
            }
        }

        ApiCrudPayload<T> payload = new() { EntityValue = value, UserName = await GetUserIdAsync() };
        try
        {
            JsonSerializerOptions options = new()
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
            var response = await _httpClient.PostAsJsonAsync(uri, payload, options, ct);
            if (response.StatusCode == HttpStatusCode.BadRequest)
                throw new Exception(await response.Content.ReadAsStringAsync(ct));
            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                response.StatusCode == HttpStatusCode.RequestTimeout)
                await SendNotification();
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

    public async Task<SubmitResult> PostValuesLogJob<T>(T value, string controllerAction, string controller = CrudApi,
        CancellationToken ct = default) where T : class?
    {
        var uri = $"{controller}{controllerAction}";
        using var _httpClientLong = new HttpClient();
        _httpClientLong.Timeout = TimeSpan.FromMinutes(10);
        _httpClientLong.BaseAddress = _httpClient.BaseAddress;

        ApiCrudPayload<T> payload = new() { EntityValue = value, UserName = await GetUserIdAsync() };
        try
        {
            JsonSerializerOptions options = new()
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };
            var response = await _httpClientLong.PostAsJsonAsync(uri, payload, options, ct);
            if (response.StatusCode == HttpStatusCode.BadRequest)
                throw new Exception(await response.Content.ReadAsStringAsync(ct));
            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                response.StatusCode == HttpStatusCode.RequestTimeout)
                await SendNotification();
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
            using var _httpClientLong = new HttpClient();
            _httpClientLong.Timeout = TimeSpan.FromMinutes(10);
            _httpClientLong.BaseAddress = _httpClient.BaseAddress;
            var response = await _httpClientLong.PostAsJsonAsync(url, values);
            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                response.StatusCode == HttpStatusCode.RequestTimeout)
                await SendNotification();
            if (response.IsSuccessStatusCode)
            {
                _alreadyNotified = false;
                var downloadresult = await _blazorDownloadFileService.DownloadFile(
                    values.FileName + " " + DateTime.Now.ToString("yyyyMMdd_HH_mm_ss") + ".xlsx",
                    await response.Content.ReadAsByteArrayAsync(), "application/octet-stream");
                if (downloadresult.Succeeded) response.Dispose();
            }
        }
        catch
        {
            // Unfortunately this HTTP API returns a 404 if there were no results, so we have to handle that separately
            //return null;
        }
    }


    public async Task ExtractAdHocQuery(RemoteDataPayload payload, CancellationToken ct)
    {
        try
        {
            var url = $"{ApiControllers.RemoteDbApi}RemoteDbExtractValues";
            using var _httpClientLong = new HttpClient();
            _httpClientLong.Timeout = TimeSpan.FromMinutes(10);
            _httpClientLong.BaseAddress = _httpClient.BaseAddress;
            var response = await _httpClientLong.PostAsJsonAsync(url, payload, ct);
            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                response.StatusCode == HttpStatusCode.RequestTimeout)
                await SendNotification();
            if (response.IsSuccessStatusCode)
            {
                _alreadyNotified = false;
                var downloadresult = await _blazorDownloadFileService.DownloadFile(
                    payload.Values.FileName + " " + DateTime.Now.ToString("yyyyMMdd_HH_mm_ss") + ".xlsx",
                    await response.Content.ReadAsStreamAsync(ct), "application/octet-stream");
                if (downloadresult.Succeeded) response.Dispose();
            }
        }
        catch (Exception)
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
            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                response.StatusCode == HttpStatusCode.RequestTimeout)
                await SendNotification();
            if (response.IsSuccessStatusCode) return (await response.Content.ReadFromJsonAsync<List<T>>())!;

            return new List<T>();
        }
        catch
        {
            return new List<T>();
        }
    }

    public async Task<T> GetUniqueValue<T>(T value, string controllerAction, string controller = CrudApi)
        where T : class?
    {
        var uri = $"{controller}{controllerAction}";
        try
        {
            var response = await _httpClient.GetAsync(uri);
            if (response.StatusCode == HttpStatusCode.Unauthorized ||
                response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                response.StatusCode == HttpStatusCode.RequestTimeout)
                await SendNotification();
            if (response.IsSuccessStatusCode) return (await response.Content.ReadFromJsonAsync<T>())!;

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
                fileContent,
                "\"file\"",
                file.Name);
            var response = await _httpClient.PostAsync($"{ApiControllers.FilesApi}Upload", content);
            return (await response.Content.ReadFromJsonAsync<SubmitResult>())!;
        }
        catch (Exception ex)
        {
            return new SubmitResult { Success = false, Message = ex.Message };
        }
    }

    public async Task<ApiResponse<T>> GetODataValues<T>(string uri, CancellationToken ct) where T : class
    {
        var response = await _httpClient.GetAsync(uri, ct);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.RequestTimeout)
            await SendNotification();
        if (response.IsSuccessStatusCode)
            return (await response.Content.ReadFromJsonAsync<ApiResponse<T>>(cancellationToken: ct))!;
        return null!;
    }
}