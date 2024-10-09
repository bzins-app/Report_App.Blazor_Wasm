namespace Report_App_WASM.Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FileType
{
    Excel = 1,
    Csv = 2,
    Json = 3,
    Xml = 4
}