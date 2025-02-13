namespace Report_App_WASM.Shared.Dashboard;

public class AppMetrics
{
    public int TasksExcecutedToday { get; set; }
    public int TasksInError { get; set; }
    public int EmailsSentToday { get; set; }
    public int EmailsInError { get; set; }
    public int FileUploadsToday { get; set; }
    public int FileUploadsInError { get; set; }
    public int ActiveReports { get; set; }
    public int ActiveAlerts { get; set; }
    public int ActiveQueries { get; set; }
    public int ActiveDataTransfer { get; set; }
    public double SizeFilesStoredLocally { get; set; }
    public int NbrOfFilesStored { get; set; }
    public int ActiveSourceDataProvider { get; set; }
    public int ActiveDestinationDataProvider { get; set; }
}