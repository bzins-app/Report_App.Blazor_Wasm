namespace Report_App_WASM.Server.Utils;

public class ExcelCreationData : IDisposable
{
    public string? FileName { get; init; }
    public bool ValidationSheet { get; init; }
    public string? ValidationText { get; init; }
    public IList<ExcelCreationDatatable>? Data { get; init; }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}