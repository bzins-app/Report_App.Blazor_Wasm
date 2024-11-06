using Report_App_WASM.Client.Utils.LanguageRessources;
using System.Reflection;

namespace Report_App_WASM.Client.Services;

public class CommonLocalizationService
{
    private readonly IStringLocalizer _localizer;

    public CommonLocalizationService(IStringLocalizerFactory factory)
    {
        var assemblyName = new AssemblyName(typeof(CommonResources).GetTypeInfo().Assembly.FullName!);
        _localizer = factory.Create(nameof(CommonResources), assemblyName.Name!);
    }

    public string Get(string? key)
    {
        if (!string.IsNullOrEmpty(key)) return _localizer[key];

        var defaultkey = "en";
        return _localizer[defaultkey];
    }
}