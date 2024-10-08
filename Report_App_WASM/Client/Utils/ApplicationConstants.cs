namespace Report_App_WASM.Client.Utils;

public static class UserAppTheme
{
    public static bool DarkTheme { get; set; } = false;
}

public static class ApiControllers
{
    public const string CrudDataApi = "api/DataCrud/";
    public const string DashboardApi = "api/Dashboard/";
    public const string BackgroudWorkerApi = "api/BackgroundWorker/";
    public const string ApplicationParametersApi = "api/ApplicationParameters/";
    public const string RemoteDbApi = "api/RemoteDb/";
    public const string UserManagerApi = "api/UserManager/";
    public const string FilesApi = "api/Files/";
    public const string DepositPathApi = "api/DepositPath/";
    public const string AuthorizeApi = "api/Authorize/";
}

public static class ApplicationInfo
{
    public const string ApplicationVersion = "202409";
    public const int VersionYear = 2024;
    public const bool Demo = true;
}