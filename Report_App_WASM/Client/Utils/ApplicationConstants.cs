namespace Report_App_WASM.Client.Utils
{
    public static class ApplicationConstants
    {
        public static string ApplicationName { get; set; } = default!;
        public static string ApplicationLogo { get; set; } = default!;
        public static bool LDAPLogin { get; set; }
    }

    public static class ApiControllers
    {
        public const string CrudDataApi = "api/DataCrud/";
        public const string DashboardApi = "api/Dashboard/";
        public const string BackgroudWorkerApi = "api/BackgroundWorker/";
        public const string ApplicationParametersApi = "api/ApplicationParameters/";
        public const string RemoteDbApi = "api/RemoteDb/";
    }
}
