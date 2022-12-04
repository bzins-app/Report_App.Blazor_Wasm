using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_App_WASM.Shared.SerializedParameters
{
    public class TaskHeaderParameters
    {
        public string Delimiter { get; set; } = ";";
        public bool UseASpecificFileNaming { get; set; } = false;
        public bool AlertOccurenceByTime { get; set; }
        public int NbrOFMinutesBeforeResendAlertEmail { get; set; } = 60;
        public int NbrOfOccurencesBeforeResendAlertEmail { get; set; } = 5;
        public string ValidationSheetText { get; set; }
        public bool UseAnExcelTemplate { get; set; } = false;
        public string ExcelTemplatePath { get; set; }
        public string ExcelFileName { get; set; }
    }
}
