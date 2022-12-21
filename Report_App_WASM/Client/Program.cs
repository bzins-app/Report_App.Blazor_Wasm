using BlazorDownloadFile;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Report_App_WASM.Client;
using Report_App_WASM.Client.Services;
using Report_App_WASM.Client.Services.Contracts;
using Report_App_WASM.Client.Services.Implementations;
using Report_App_WASM.Client.Services.States;
using Report_App_WASM.Client.Utils;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<IdentityAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<IdentityAuthenticationStateProvider>());
builder.Services.AddScoped<IAuthorizeApi, AuthorizeApi>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<DataInteractionService>();
builder.Services.AddScoped<ApplicationService>();
builder.Services.AddMudServices();

builder.Services.AddLocalization(options => options.ResourcesPath = "Utils/LanguageRessources");
builder.Services.AddSingleton<CommonLocalizationService>();
builder.Services.AddBlazorDownloadFile(ServiceLifetime.Scoped);


var host = builder.Build();

//Setting culture of the application
var jsInterop = host.Services.GetRequiredService<IJSRuntime>();
var result = await jsInterop.InvokeAsync<string>("cultureInfo.get");
CultureInfo culture;
if (result != null)
{
    culture = new CultureInfo(result);
}
else
{
    var browserLanguage = await jsInterop.InvokeAsync<string>("getBrowserLanguage");
    culture = new CultureInfo(browserLanguage.Substring(0,2));
    await jsInterop.InvokeVoidAsync("cultureInfo.set", browserLanguage.Substring(0, 2));
}
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;
var resultTheme = await jsInterop.InvokeAsync<string>("AppTheme.get");
if (resultTheme != null)
{
    UserAppTheme.DarkTheme = resultTheme=="Dark" ? true : false;
}
else
{
    await jsInterop.InvokeVoidAsync("AppTheme.set", "Light");
}


await builder.Build().RunAsync();