using Microsoft.Extensions.Localization;
using Report_App_WASM.Shared.LanguageResources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Report_App_WASM.Shared.Services
{
    public class CommonLocalizationService
    {
        private readonly IStringLocalizer localizer;
        public CommonLocalizationService(IStringLocalizerFactory factory)
        {
            var assemblyName = new AssemblyName(typeof(CommonResources).GetTypeInfo().Assembly.FullName);
            localizer = factory.Create(nameof(CommonResources), assemblyName.Name);
        }

        public string Get(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                return localizer[key];
            }
            else
            {
                string defaultkey = "en";
                return localizer[defaultkey];
            }
        }
    }
}
