namespace Report_App_WASM.Shared.SerializedParameters;

public class ScheduledTaskQueryParameters
{
    public string? FileName { get; set; }
    public string? EncodingType { get; set; } = "UTF8";
    public ExcelTemplate ExcelTemplate { get; set; } = new();
    public bool SeparateExcelFile { get; set; } = false;
    public bool RemoveHeader { get; set; } = false;
    public bool GenerateIfEmpty { get; set; } = false;
    public bool AddValidationSheet { get; set; } = false;
    public string? ExcelTabName { get; set; }
    public string? DataTransferTargetTableName { get; set; }
    public bool DataTransferCreateTable { get; set; } = false;
    public bool DataTransferUsePk { get; set; } = false;
    public string? DataTransferCommandBehaviour { get; set; }
    public List<string?>? DataTransferPk { get; set; } = new();
}