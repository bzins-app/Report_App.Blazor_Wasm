﻿namespace Report_App_WASM.Client.Services;

public class ApplicationService
{
    private readonly IdentityAuthenticationStateProvider _authenticationStateProvider;
    private readonly HttpClient _httpClient;
    private readonly CommonLocalizationService _localizer;
    private ApplicationConstantsValues? _constantsValuesCache;

    public ApplicationService(CommonLocalizationService localizer, HttpClient httpClient,
        IdentityAuthenticationStateProvider authenticationStateProvider)
    {
        _localizer = localizer;
        _httpClient = httpClient;
        _authenticationStateProvider = authenticationStateProvider;
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
            TheSameDateWith = _localizer.Get("The same date with"),
            ItemsPerPage = _localizer.Get("Rows per page")
        };
    }

    private string GetUniqueName(string fileName)
    {
        fileName = Path.GetFileName(fileName);
        return Path.GetFileNameWithoutExtension(fileName)
               + "_" + Guid.NewGuid().ToString()[..4]
               + Path.GetExtension(fileName);
    }

    public async Task<Tuple<string, string>?> GetFilePath(string fileNameToUrl, bool unique = true)
    {
        var fileName = unique ? GetUniqueName(fileNameToUrl) : fileNameToUrl;
        var uri = $"{ApiControllers.ApplicationParametersApi}GetUploadedFilePath?fileName={fileName}";
        return await _httpClient.GetFromJsonAsync<Tuple<string, string>>(uri);
    }

    private async Task<string?> GetUserNameAsync()
    {
        return (await _authenticationStateProvider.GetAuthenticationStateAsync())?.User?.Identity?.Name;
    }

    public async Task<bool> GetUserTheme()
    {
        var user = await _authenticationStateProvider.GetUserInfo();
        return user?.AppTheme == "Dark";
    }

    public async Task<IdentityDefaultOptions?> GetIdentityOptionsAsync()
    {
        var uri = $"{ApiControllers.AuthorizeApi}GetIdentityOptions";
        return await _httpClient.GetFromJsonAsync<IdentityDefaultOptions>(uri);
    }

    public async Task<ApplicationConstantsValues?> GetApplicationConstantsValues()
    {
        if (_constantsValuesCache == null)
        {
            _constantsValuesCache = await _httpClient.GetFromJsonAsync<ApplicationConstantsValues>(
                $"{ApiControllers.ApplicationParametersApi}ApplicationConstants");
        }

        return _constantsValuesCache;
    }
}