namespace Report_App_WASM.Shared.ApiExchanges
{
    public class TaksLogsValues
    {
        public DateTime Date { get; init; }
        public string? ActivityName { get; init; }
        public string? TypeTask { get; init; }
        public int TotalDuration { get; init; }
        public int NbrTasks { get; init; }
        public int NbrErrors { get; init; }
    }

    public class TaksSystemValues
    {
        public DateTime Date { get; init; }
        public int NbrWarnings { get; init; }
        public int NbrErrors { get; init; }
        public int NbrCriticals { get; init; }
    }

    public class EmailsLogsValues
    {
        public DateTime Date { get; init; }
        public int NbrEmails { get; init; }
        public int NbrErrors { get; init; }
        public int TotalDuration { get; init; }
    }

    public class StorageData
    {
        public string? ReportName { get; init; }
        public double FileSizeInMb { get; init; }
    }
}

