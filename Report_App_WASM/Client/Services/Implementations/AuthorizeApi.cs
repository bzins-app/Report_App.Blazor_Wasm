using System.Net;
using Report_App_WASM.Client.Services.Contracts;

namespace Report_App_WASM.Client.Services.Implementations;

public class AuthorizeApi : IAuthorizeApi
{
    private readonly HttpClient _httpClient;

    public AuthorizeApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task Login(LoginParameters loginParameters)
    {
        var result = await _httpClient.PostAsJsonAsync("api/Authorize/Login", loginParameters);
        await HandleResponse(result);
    }

    public async Task LoginLdap(LoginParameters loginParameters)
    {
        var result = await _httpClient.PostAsJsonAsync("api/Authorize/LdapLogin", loginParameters);
        await HandleResponse(result);
    }

    public async Task LoginDemo(LoginParameters loginParameters)
    {
        //var stringContent = new StringContent(JsonSerializer.Serialize(loginParameters), Encoding.UTF8, "application/json");
        var result = await _httpClient.PostAsJsonAsync("api/Authorize/DemoLogin", loginParameters);
        if (result.StatusCode == HttpStatusCode.BadRequest)
            throw new Exception(await result.Content.ReadAsStringAsync());
        result.EnsureSuccessStatusCode();
    }

    public async Task Logout()
    {
        var result = await _httpClient.PostAsync("api/Authorize/Logout", null);
        result.EnsureSuccessStatusCode();
    }

    public async Task Register(RegisterParameters registerParameters)
    {
        var result = await _httpClient.PostAsJsonAsync("api/Authorize/Register", registerParameters);
        await HandleResponse(result);
    }

    public async Task<UserInfo?> GetUserInfo()
    {
        return await _httpClient.GetFromJsonAsync<UserInfo>("api/Authorize/UserInfo");
    }

    private static async Task HandleResponse(HttpResponseMessage result)
    {
        if (result.StatusCode == HttpStatusCode.BadRequest)
            throw new Exception(await result.Content.ReadAsStringAsync());
        result.EnsureSuccessStatusCode();
    }
}
