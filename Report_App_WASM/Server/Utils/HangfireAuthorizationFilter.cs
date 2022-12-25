using Hangfire.Dashboard;

namespace Report_App_WASM.Server.Utils
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Allow all authenticated users to see the Dashboard (potentially dangerous).
            //return httpContext.User.Identity.IsAuthenticated;

            //WithRole
            return httpContext.User.IsInRole("Admin");
        }
    }

    public class HangfireAuthorizationFilterRead : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Allow all authenticated users to see the Dashboard (potentially dangerous).
            return httpContext.User.Identity!.IsAuthenticated;

            //WithRole
            //return httpContext.User.IsInRole("Admin");
        }
    }
}
