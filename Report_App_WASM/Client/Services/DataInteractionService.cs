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
        return (await _authenticationStateProvider.GetAuthenticationStateAsync())?.User?.Identity?.Name;
    }

    public event Action<bool>? NotifyNotConnected;

    private Task SendNotification()
    {
        if (!_alreadyNotified)
        {
            _alreadyNotified = true;
            NotifyNotConnected?.Invoke(true);
        }

        return Task.CompletedTask;
    }

    private async Task<SubmitResult> HandleResponse(HttpResponseMessage response, CancellationToken ct = default)
    {
        if (response.StatusCode == HttpStatusCode.BadRequest)
            throw new Exception(await response.Content.ReadAsStringAsync(ct));
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.ServiceUnavailable or HttpStatusCode.RequestTimeout)
            await SendNotification();
        if (response.IsSuccessStatusCode)
        {
            _alreadyNotified = false;
            return (await response.Content.ReadFromJsonAsync<SubmitResult>(cancellationToken: ct))!;
        }

        return new SubmitResult { Success = false };
    }

    private async Task<SubmitResult> PostDataAsync<T>(string uri, T payload, HttpClient client, CancellationToken ct = default)
    {
        try
        {
            var response = await client.PostAsJsonAsync(uri, payload, new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.IgnoreCycles }, ct);
            return await HandleResponse(response, ct);
        }
        catch (Exception ex)
        {
            return new SubmitResult { Success = false, Message = ex.Message };
        }
    }

    public Task<SubmitResult> PostValues<T>(T value, string controllerAction, string controller = CrudApi, CancellationToken ct = default) where T : class?
    {
        var uri = $"{controller}{controllerAction}";
        var payload = new ApiCrudPayload<T> { EntityValue = value, UserName = GetUserIdAsync().Result };
        return PostDataAsync(uri, payload, _httpClient, ct);
    }

    public Task<SubmitResult> PostValuesLogJob<T>(T value, string controllerAction, string controller = CrudApi, CancellationToken ct = default) where T : class?
    {
        var uri = $"{controller}{controllerAction}";
        var payload = new ApiCrudPayload<T> { EntityValue = value, UserName = GetUserIdAsync().Result };
        var httpClientLong = new HttpClient { Timeout = TimeSpan.FromMinutes(10), BaseAddress = _httpClient.BaseAddress };
        return PostDataAsync(uri, payload, httpClientLong, ct);
    }

    private async Task HandleDownloadResponse(HttpResponseMessage response, string fileName)
    {
        if (response.IsSuccessStatusCode)
        {
            _alreadyNotified = false;
            var downloadResult = await _blazorDownloadFileService.DownloadFile(fileName, await response.Content.ReadAsByteArrayAsync(), "application/octet-stream");
            if (downloadResult.Succeeded) response.Dispose();
        }
    }

    public async Task ExtractGridLogs(ODataExtractPayload values)
    {
        var url = "odata/ExtractLogs";
        var httpClientLong = new HttpClient { Timeout = TimeSpan.FromMinutes(10), BaseAddress = _httpClient.BaseAddress };
        var response = await httpClientLong.PostAsJsonAsync(url, values);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.ServiceUnavailable or HttpStatusCode.RequestTimeout)
            await SendNotification();
        await HandleDownloadResponse(response, $"{values.FileName} {DateTime.Now:yyyyMMdd_HH_mm_ss}.xlsx");
    }

    public async Task ExtractAdHocQuery(RemoteDataPayload payload, CancellationToken ct)
    {
        var url = $"{ApiControllers.RemoteDbApi}RemoteDbExtractValues";
        var httpClientLong = new HttpClient { Timeout = TimeSpan.FromMinutes(10), BaseAddress = _httpClient.BaseAddress };
        var response = await httpClientLong.PostAsJsonAsync(url, payload, ct);
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.ServiceUnavailable or HttpStatusCode.RequestTimeout)
            await SendNotification();
        await HandleDownloadResponse(response, $"{payload.Values.FileName} {DateTime.Now:yyyyMMdd_HH_mm_ss}.xlsx");
    }

    public async Task<List<T>> GetValues<T>(string controllerAction, string controller = CrudApi) where T : class
    {
        var uri = $"{controller}{controllerAction}";
        try
        {
            var response = await _httpClient.GetAsync(uri);
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.ServiceUnavailable or HttpStatusCode.RequestTimeout)
                await SendNotification();
            return response.IsSuccessStatusCode ? (await response.Content.ReadFromJsonAsync<List<T>>())! : new List<T>();
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
            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.ServiceUnavailable or HttpStatusCode.RequestTimeout)
                await SendNotification();
            return response.IsSuccessStatusCode ? (await response.Content.ReadFromJsonAsync<T>())! : value;
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
            var fileContent = new StreamContent(file.OpenReadStream(maxFileSize))
            {
                Headers = { ContentType = new MediaTypeHeaderValue(file.ContentType) }
            };
            content.Add(fileContent, "\"file\"", file.Name);
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
        if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.ServiceUnavailable or HttpStatusCode.RequestTimeout)
            await SendNotification();
        return response.IsSuccessStatusCode ? (await response.Content.ReadFromJsonAsync<ApiResponse<T>>(cancellationToken: ct))! : null!;
    }
}
