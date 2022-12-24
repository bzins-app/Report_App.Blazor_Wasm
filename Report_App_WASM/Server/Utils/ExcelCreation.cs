using System.Data;
using Report_App_WASM.Shared.SerializedParameters;

namespace Report_App_WASM.Server.Utils
{
    public struct ExcelCreationDatatable : IDisposable
    {
        public string? TabName;
        public ExcelTemplate ExcelTemplate;
        public DataTable Data;

        public ExcelCreationDatatable(string? tabName, ExcelTemplate excelTemplate, DataTable data)
        {
            TabName = tabName;
            ExcelTemplate = excelTemplate;
            Data = data;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    public class ExcelCreationData : IDisposable
    {
        public string? FileName { get; set; }
        public bool ValidationSheet { get; set; }
        public string? ValidationText { get; set; }
        public IList<ExcelCreationDatatable>? Data { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    //public class ExcelTemplate : IDisposable
    //{
    //    public string ExcelTabName { get; set; }
    //    public string ExcelTemplateCellReference { get; set; } = "A1";
    //    public bool UseAnExcelDataTable { get; set; } = false;
    //    public string ExcelDataTableName { get; set; }

    //    public void Dispose()
    //    {
    //        GC.SuppressFinalize(this);
    //    }
    //}
}
