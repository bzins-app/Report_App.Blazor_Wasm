namespace Report_App_WASM.Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskType
{
    Report = 0,
    Alert = 1,
    DataTransfer = 2
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LogType
{
    NotSet = 0,
    TaskLog = 10,
    TaskStepLog = 20,
    QueryExecutionLog = 30,
    EmailLog = 40,
    ReportGenerationLog = 50
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FileGenerationType
{
    LocalCopy = 10,
    DirectToFolder = 20,
    Ftp = 30,
    Sftp = 40
}