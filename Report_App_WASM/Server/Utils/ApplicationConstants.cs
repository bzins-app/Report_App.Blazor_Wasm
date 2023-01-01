namespace Report_App_WASM.Server.Utils
{
    public static class ApplicationConstants
    {
        public static string? ApplicationName { get; set; }
        public static string? ApplicationLogo { get; set; }
        public static bool LdapLogin { get; set; }
        public static bool ActivateTaskSchedulerModule { get; set; }
        public static bool ActivateAdHocQueriesModule { get; set; }
        public static bool WindowsEnv { get; set; }
    }
}
