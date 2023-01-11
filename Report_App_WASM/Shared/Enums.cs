using System.Text.Json.Serialization;

namespace Report_App_WASM.Shared
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TypeDb
    {
        Oracle = 1,
        SqlServer = 2,
        MySql = 3,
        MariaDb = 4,
        Db2 = 5
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FileType
    {
        Excel = 1,
        Csv = 2,
        Json = 3,
        Xml = 4
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TaskType
    {
        Report = 0,
        Alert = 1,
        DataTransfer = 2
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ActivityType
    {
        SourceDb = 0,
        TargetDb = 1
    }

    public enum DataTransferBasicBehaviour
    {
        Append,
        Recreate
    }

    public enum DataTransferAdvancedBehaviour
    {
        Insert,
        InsertOrUpdate,
        InsertOrUpdateOrDelete
    }

    public enum EncodingType
    {
        Ascii,
        Utf8,
        Utf16,
        Utf32,
        Latin1
    }

    public enum CrudAction
    {
        Create,
        Update,
        Delete
    }

    public enum TypeConfiguration
    {
        Grid = 1,
        PitvotReport = 2
    }
}
