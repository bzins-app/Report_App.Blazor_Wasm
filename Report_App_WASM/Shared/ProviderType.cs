namespace Report_App_WASM.Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProviderType
{
    SourceDatabase = 0,
    TargetDatabase = 1
}