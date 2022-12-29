using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Report_App_WASM.Server.Utils
{
    public class SwaggerFilters : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            //remove paths those start with /api/abp prefix
            swaggerDoc.Paths
                .Where(x => x.Key.ToLowerInvariant().StartsWith("/api")|| x.Key.ToLowerInvariant().StartsWith("/odata"))
                .ToList()
                .ForEach(x => swaggerDoc.Paths.Remove(x.Key));
        }
    }
}
