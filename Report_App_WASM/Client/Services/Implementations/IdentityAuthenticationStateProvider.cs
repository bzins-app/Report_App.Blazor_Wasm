using Report_App_WASM.Client.Services.Contracts;

namespace Report_App_WASM.Client.Services.Implementations;

public class IdentityAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IAuthorizeApi _authorizeApi;
    private UserInfo? _userInfoCache;

    public IdentityAuthenticationStateProvider(IAuthorizeApi authorizeApi)
    {
        _authorizeApi = authorizeApi;
    }

    public async Task Login(LoginParameters loginParameters)
    {
        await _authorizeApi.Login(loginParameters);
        NotifyAuthenticationStateChanged(Task.FromResult(await GetAuthenticationStateAsync()));
    }

    public async Task LoginLdap(LoginParameters loginParameters)
    {
        await _authorizeApi.LoginLdap(loginParameters);
        NotifyAuthenticationStateChanged(Task.FromResult(await GetAuthenticationStateAsync()));
    }

    public async Task Register(RegisterParameters registerParameters)
    {
        await _authorizeApi.Register(registerParameters);
        NotifyAuthenticationStateChanged(Task.FromResult(await GetAuthenticationStateAsync()));
    }

    public async Task Logout()
    {
        await _authorizeApi.Logout();
        _userInfoCache = null;
        NotifyAuthenticationStateChanged(Task.FromResult(await GetAuthenticationStateAsync()));
    }

    public async Task<UserInfo?> GetUserInfo()
    {
        if (_userInfoCache is { IsAuthenticated: true }) return _userInfoCache;
        _userInfoCache = await _authorizeApi.GetUserInfo();
        return _userInfoCache;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity();
        try
        {
            var userInfo = await GetUserInfo();
            if (userInfo?.IsAuthenticated == true)
            {
                var claims = new[] { new Claim(ClaimTypes.Name, userInfo.UserName!) }
                    .Concat(userInfo.ExposedClaims!.Select(c => new Claim(c.Type!, c.Value!)));
                identity = new ClaimsIdentity(claims, "Server authentication");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine(@"Request failed:" + ex);
        }

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }
}
