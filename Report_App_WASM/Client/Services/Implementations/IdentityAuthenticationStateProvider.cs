using Microsoft.AspNetCore.Components.Authorization;
using Report_App_WASM.Client.Services.Contracts;
using Report_App_WASM.Shared;
using System.Security.Claims;

namespace Report_App_WASM.Client.Services.Implementations
{
    public class IdentityAuthenticationStateProvider : AuthenticationStateProvider
    {
        private UserInfo? _userInfoCache;
        private readonly IAuthorizeApi _authorizeApi;

        public IdentityAuthenticationStateProvider(IAuthorizeApi authorizeApi)
        {
            _authorizeApi = authorizeApi;
        }

        public async Task Login(LoginParameters loginParameters)
        {
            await _authorizeApi.Login(loginParameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task LoginLdap(LoginParameters loginParameters)
        {
            await _authorizeApi.LoginLdap(loginParameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task Register(RegisterParameters registerParameters)
        {
            await _authorizeApi.Register(registerParameters);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task Logout()
        {
            await _authorizeApi.Logout();
            _userInfoCache = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task<UserInfo?> GetUserInfo()
        {
            if (_userInfoCache != null && _userInfoCache.IsAuthenticated) return _userInfoCache;
            _userInfoCache = await _authorizeApi.GetUserInfo();
            return _userInfoCache;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity();
            try
            {
                var userInfo = await GetUserInfo();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                if (userInfo.IsAuthenticated)
                {
#pragma warning disable CS8604 // Possible null reference argument for parameter 'value' in 'Claim.Claim(string type, string value)'.
#pragma warning disable CS8604 // Possible null reference argument for parameter 'value' in 'Claim.Claim(string type, string value)'.
                    var claims = new[] { new Claim(ClaimTypes.Name, userInfo.UserName) }.Concat(userInfo.ExposedClaims!.Select(c => new Claim(c.Type!, c.Value)));
#pragma warning restore CS8604 // Possible null reference argument for parameter 'value' in 'Claim.Claim(string type, string value)'.
#pragma warning restore CS8604 // Possible null reference argument for parameter 'value' in 'Claim.Claim(string type, string value)'.
                    identity = new(claims, "Server authentication");
                }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("Request failed:" + ex);
            }

            return new(new(identity));
        }
    }
}
