namespace Report_App_WASM.Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ActivityType
{
    SourceDb = 0,
    TargetDb = 1
}