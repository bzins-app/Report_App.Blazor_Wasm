using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Localization;
using MudBlazor.Services;
using Report_App_WASM.Client;
using Report_App_WASM.Client.Services.Contracts;
using Report_App_WASM.Client.Services.Implementations;
using Report_App_WASM.Client.Services.States;
using System.Globalization;
using BlazorDownloadFile;
using Report_App_WASM.Client.Services;
using Microsoft.JSInterop;
using Report_App_WASM.Client.Pages.Parameters;

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
    culture = new CultureInfo("en");
    await jsInterop.InvokeVoidAsync("cultureInfo.set", "en");
}
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

await builder.Build().RunAsync();
