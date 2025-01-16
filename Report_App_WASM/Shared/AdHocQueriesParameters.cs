namespace Report_App_WASM.Shared;

public class AdHocQueriesParameters
{
    public bool CalculateTotalItems { get; set; }
    public bool UsePivotTable { get; set; }
    public int PivotTableNbrOfColumnsMax { get; set; } = 10;
    public int PivotTableMaxRowsFetched { get; set; } = 20000;
    public bool PivotTableAsDefaultView { get; set; }
    public string PivotTableDefaultConfig { get; set; } = string.Empty;
}