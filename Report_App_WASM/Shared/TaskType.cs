namespace Report_App_WASM.Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskType
{
    Report = 0,
    Alert = 1,
    DataTransfer = 2
}