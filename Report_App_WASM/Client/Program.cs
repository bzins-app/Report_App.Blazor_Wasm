using System.Globalization;
using ApexCharts;
using BlazorDownloadFile;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Report_App_WASM.Client.Services.Contracts;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<IdentityAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    s => s.GetRequiredService<IdentityAuthenticationStateProvider>());
builder.Services.AddScoped<IAuthorizeApi, AuthorizeApi>();
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<DataInteractionService>();
builder.Services.AddScoped<ApplicationService>();
builder.Services.AddMudServices();
builder.Services.AddApexCharts();

builder.Services.AddLocalization(options => options.ResourcesPath = "Utils/LanguageRessources");
builder.Services.AddSingleton<CommonLocalizationService>();
builder.Services.AddBlazorDownloadFile();
builder.Logging.SetMinimumLevel(LogLevel.Warning);

var host = builder.Build();

await SetCultureAndThemeAsync(host.Services);

await host.RunAsync();

async Task SetCultureAndThemeAsync(IServiceProvider services)
{
    var jsInterop = services.GetRequiredService<IJSRuntime>();
    var result = await jsInterop.InvokeAsync<string>("cultureInfo.get");
    CultureInfo culture;
    if (result != null)
    {
        culture = new CultureInfo(result);
    }
    else
    {
        var browserLanguage = await jsInterop.InvokeAsync<string>("getBrowserLanguage");
        culture = new CultureInfo(browserLanguage[..2]);
        await jsInterop.InvokeVoidAsync("cultureInfo.set", browserLanguage[..2]);
    }

    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;

    var resultTheme = await jsInterop.InvokeAsync<string>("AppTheme.get");
    if (resultTheme != null)
        UserAppTheme.DarkTheme = resultTheme == "Dark";
    else
        await jsInterop.InvokeVoidAsync("AppTheme.set", "Light");
}