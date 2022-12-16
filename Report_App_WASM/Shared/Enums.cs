namespace Report_App_WASM.Shared
{
    public enum DBType
    {
        Oracle=1,
        SQLServer=2,       
        MySQL=3,
        MariaDB=4,
        DB2=5
    }

    public enum FileType
    {
        Excel=1,
        CSV=2,
        Json=3,
        XML=4
    }

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
