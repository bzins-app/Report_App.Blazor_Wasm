using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Localization;
using MudBlazor.Services;
using Report_App_WASM.Client;
using Report_App_WASM.Client.Services.Contracts;
using Report_App_WASM.Client.Services.Implementations;
using Report_App_WASM.Client.Services.States;
using Report_App_WASM.Shared.Services;
using Report_App_WASM.Shared.LanguageResources;
using System.Globalization;
using BlazorDownloadFile;
using Report_App_WASM.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<IdentityAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<IdentityAuthenticationStateProvider>());
builder.Services.AddScoped<IAuthorizeApi, AuthorizeApi>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<DataEntityService>();
builder.Services.AddScoped<ApplicationService>();
builder.Services.AddMudServices();

builder.Services.AddLocalization(options => options.ResourcesPath = "LanguageRessources");
builder.Services.AddSingleton<CommonLocalizationService>();
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
     {
                    new CultureInfo("fr"),
                    new CultureInfo("en"),
                    new CultureInfo("de"),
                    new CultureInfo("nl"),
                 };
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});
builder.Services.AddBlazorDownloadFile(ServiceLifetime.Scoped);
await builder.Build().RunAsync();
