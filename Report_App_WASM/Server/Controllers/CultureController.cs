using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Report_App_WASM.Server.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("[controller]/[action]")]
    public class CultureController : Controller, IDisposable
    {

        [HttpPost]
        public IActionResult SetCulture(string? culture, string redirectUri)
        {
            if (culture != null)
            {
                HttpContext.Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(
                        new RequestCulture(culture)));
            }

            return LocalRedirect(redirectUri);
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
