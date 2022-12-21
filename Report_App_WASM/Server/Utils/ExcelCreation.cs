using Report_App_WASM.Shared.SerializedParameters;
using System.Data;

namespace Report_App_WASM.Server.Utils
{
    public struct ExcelCreationDatatable : IDisposable
    {
        public string TabName;
        public ExcelTemplate ExcelTemplate;
        public DataTable Data;

        public ExcelCreationDatatable(string TabName, ExcelTemplate ExcelTemplate, DataTable Data)
        {
            this.TabName = TabName;
            this.ExcelTemplate = ExcelTemplate;
            this.Data = Data;
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
