namespace Report_App_WASM.Server.Utils.FIles;

public class ExcelCreationData : IDisposable
{
    public string FileName { get; init; }= string.Empty;
    public bool ValidationSheet { get; init; }
    public string? ValidationText { get; init; }
    public IList<ExcelCreationDatatable>? Data { get; init; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}