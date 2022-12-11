using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_App_WASM.Shared.ApiResponse
{
    public class ODataExtractPayload
    {
        public string FunctionName { get; set; }
        public string FilterValues { get; set; }
        public string SortValues { get; set; }
        public string FileName { get; set; }
        public string TabName { get; set; } = "Data";
        public int MaxResult { get; set; } = 100000;
    }
}
