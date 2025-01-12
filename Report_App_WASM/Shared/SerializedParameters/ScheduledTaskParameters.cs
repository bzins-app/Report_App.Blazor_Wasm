namespace Report_App_WASM.Shared.SerializedParameters;

public class ScheduledTaskParameters
{
    public string? Delimiter { get; set; } = ";";
    public bool UseASpecificFileNaming { get; set; } = false;
    public bool AlertOccurenceByTime { get; set; }
    public int NbrOfMinutesBeforeResendAlertEmail { get; set; } = 60;
    public int NbrOfOccurencesBeforeResendAlertEmail { get; set; } = 5;
    public string? ValidationSheetText { get; set; }
    public bool UseAnExcelTemplate { get; set; } = false;
    public string? ExcelTemplatePath { get; set; }
    public string? ExcelFileName { get; set; }
    public long DataTransferId { get; set; }
}