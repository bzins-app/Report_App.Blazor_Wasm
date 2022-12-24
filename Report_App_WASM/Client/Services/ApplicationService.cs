using Blazor.SimpleGrid;
using Report_App_WASM.Client.Services.States;
using Report_App_WASM.Client.Utils;
using System.Net.Http.Json;

namespace Report_App_WASM.Client.Services
{
    public class ApplicationService
    {
        private readonly CommonLocalizationService _Localizer;
        private readonly HttpClient _httpClient;
        private readonly IdentityAuthenticationStateProvider _authenticationStateProvider;

        public ApplicationService(CommonLocalizationService Localizer, HttpClient httpClient, IdentityAuthenticationStateProvider authenticationStateProvider)
        {
            _Localizer = Localizer;
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public SimpleGridFieldsContent? GetGridTranslations()
        {
            return new SimpleGridFieldsContent
            {
                LessThan = _Localizer.Get("Less than"),
                Condition = _Localizer.Get("Condition"),
                Contains = _Localizer.Get("Contains"),
                EndsWith = _Localizer.Get("Ends with"),
                Equals = _Localizer.Get("Equals"),
                GoToFirstPage = _Localizer.Get("Go to first page"),
                GoToLastPage = _Localizer.Get("Go to last page"),
                GoToNextage = _Localizer.Get("Go to next page"),
                GoToPreviousPage = _Localizer.Get("Go to previous page"),
                GreaterThan = _Localizer.Get("Greater than"),
                GreaterThanOrEquals = _Localizer.Get("Greater than or equals"),
                IsNotNull = _Localizer.Get("Is not null"),
                IsNull = _Localizer.Get("Is null"),
                Items = _Localizer.Get("items"),
                LessThanOrEquals = _Localizer.Get("Less than or equals"),
                NotContains = _Localizer.Get("Not contains"),
                NotEquals = _Localizer.Get("Not equals"),
                Of = _Localizer.Get("of"),
                Page = _Localizer.Get("Page"),
                Reset = _Localizer.Get("Reset"),
                StartsWith = _Localizer.Get("Starts with"),
                TheSameDateWith = _Localizer.Get("The same date with"),
                ItemsPerPage=_Localizer.Get("Rows per page")
            };
        }

        private string GetUniqueName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            return Path.GetFileNameWithoutExtension(fileName)
                   + "_" + Guid.NewGuid().ToString()[..4]
                   + Path.GetExtension(fileName);
        }

        public async Task<Tuple<string, string>> GetFilePath(string fileNameToUrl, bool unique = true)
        {
            var fileName = fileNameToUrl;
            if (unique)
            {
                fileName = GetUniqueName(fileNameToUrl);
            }
            var uri = $"{ApiControllers.ApplicationParametersApi}GetUploadedFilePath?fileName={fileName}";
            return await _httpClient.GetFromJsonAsync<Tuple<string, string>>(uri);
        }

        private async Task<string> GetUserIdAsync()
        {
            return (await _authenticationStateProvider.GetAuthenticationStateAsync())?.User?.Identity?.Name;// FindFirst(ClaimTypes.NameIdentifier).Value;
        }

        public async Task<bool> GetUserTheme()
        {
            var user = await _authenticationStateProvider.GetUserInfo();
            if (user == null)
            {
                if (user.AppTheme == "Dark")
                    return true;
                else return false;
            }
            else return false;
        }
    }
}
