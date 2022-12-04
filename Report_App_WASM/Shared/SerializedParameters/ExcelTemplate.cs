using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_App_WASM.Shared.SerializedParameters
{
    public class ExcelTemplate
    {
        public string ExcelTabName { get; set; }
        public string ExcelTemplateCellReference { get; set; } = "A1";
        public bool UseAnExcelDataTable { get; set; } = false;
        public string ExcelDataTableName { get; set; }
    }
}
