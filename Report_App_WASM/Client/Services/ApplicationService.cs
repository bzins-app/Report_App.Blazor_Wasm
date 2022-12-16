using Blazor.SimpleGrid;
using Microsoft.AspNetCore.Components.Authorization;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.Services;
using System.Net.Http.Json;

namespace Report_App_WASM.Client.Services
{
    public class ApplicationService
    {
        private readonly CommonLocalizationService _localizer;
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _AuthenticationStateProvider;

        public ApplicationService(CommonLocalizationService localizer, HttpClient httpClient, AuthenticationStateProvider AuthenticationStateProvider)
        {
            _localizer = localizer;
            _httpClient = httpClient;
            _AuthenticationStateProvider = AuthenticationStateProvider;
        }

        public SimpleGridFieldsContent GetGridTranslations()
        {
            return new SimpleGridFieldsContent
            {
                LessThan = _localizer.Get("Less than"),
                Condition = _localizer.Get("Condition"),
                Contains = _localizer.Get("Contains"),
                EndsWith = _localizer.Get("Ends with"),
                Equals = _localizer.Get("Equals"),
                GoToFirstPage = _localizer.Get("Go to first page"),
                GoToLastPage = _localizer.Get("Go to last page"),
                GoToNextage = _localizer.Get("Go to next page"),
                GoToPreviousPage = _localizer.Get("Go to previous page"),
                GreaterThan = _localizer.Get("Greater than"),
                GreaterThanOrEquals = _localizer.Get("Greater than or equals"),
                IsNotNull = _localizer.Get("Is not null"),
                IsNull = _localizer.Get("Is null"),
                Items = _localizer.Get("items"),
                LessThanOrEquals = _localizer.Get("Less than or equals"),
                NotContains = _localizer.Get("Not contains"),
                NotEquals = _localizer.Get("Not equals"),
                Of = _localizer.Get("of"),
                Page = _localizer.Get("Page"),
                Reset = _localizer.Get("Reset"),
                StartsWith = _localizer.Get("Starts with"),
                TheSameDateWith = _localizer.Get("The same date with")
            };
        }



        private async Task<string> GetUserIdAsync()
        {
            return (await _AuthenticationStateProvider.GetAuthenticationStateAsync())?.User?.Identity?.Name;// FindFirst(ClaimTypes.NameIdentifier).Value;
        }

        public async Task<SubmitResult> PostValues(string uri, ApiBackgrounWorkerdPayload value)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(uri, value);
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

    }
}
