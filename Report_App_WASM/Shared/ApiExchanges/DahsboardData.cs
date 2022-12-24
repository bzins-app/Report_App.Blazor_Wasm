using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_App_WASM.Shared
{
    public class TaksLogsValues
    {
        public DateTime Date { get; set; }
        public string? ActivityName { get; set; }
        public string? TypeTask { get; set; }
        public int TotalDuration { get; set; }
        public int NbrTasks { get; set; }
        public int NbrErrors { get; set; }
    }

    public class TaksSystemValues
    {
        public DateTime Date { get; set; }
        public int NbrWarnings { get; set; }
        public int NbrErrors { get; set; }
        public int NbrCriticals { get; set; }
    }

    public class EmailsLogsalues
    {
        public DateTime Date { get; set; }
        public int NbrEmails { get; set; }
        public int NbrErrors { get; set; }
        public int TotalDuration { get; set; }
    }
}

