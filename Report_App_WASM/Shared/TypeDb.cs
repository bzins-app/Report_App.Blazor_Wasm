namespace Report_App_WASM.Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TypeDb
{
    Oracle = 1,
    SqlServer = 2,
    MySql = 3,
    MariaDb = 4,
    PostgreSql = 6,
    OlebDb = 10
}