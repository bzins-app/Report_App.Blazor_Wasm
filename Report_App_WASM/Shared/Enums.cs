using System.Text.Json.Serialization;

namespace Report_App_WASM.Shared
{

    public static class StaticAppLists
    {
        public static List<string> GetDataTransferBasicBehaviour = Enum.GetValues(typeof(DataTransferBasicBehaviour)).Cast<DataTransferBasicBehaviour>().Select(v => v.ToString()).ToList();
        public static List<string> GetDataTransferAdvancedBehaviour = Enum.GetValues(typeof(DataTransferAdvancedBehaviour)).Cast<DataTransferAdvancedBehaviour>().Select(v => v.ToString()).ToList();
        public static List<string> GetEncodingType = Enum.GetValues(typeof(EncodingType)).Cast<EncodingType>().Select(v => v.ToString()).ToList();
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TypeDb
    {
        Oracle=1,
        SQLServer=2,       
        MySQL=3,
        MariaDB=4,
        DB2=5
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FileType
    {
        Excel=1,
        CSV=2,
        Json=3,
        XML=4
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TaskType
    {
        Report=0,
        Alert=1,
        DataTransfer=2
    }

    public enum ActivityType
    {
        SourceDB=0,
        TargetDB=1
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
        ASCII,
        UTF8,
        UTF16,
        UTF32,
        Latin1
    }

    public enum CrudAction
    {
        Create,
        Update,
        Delete
    }
}
