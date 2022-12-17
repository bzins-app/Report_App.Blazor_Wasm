using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_App_WASM.Shared
{
    public  class AppMetrics
    {
        public int NbrOfTasksExcecutedToday { get; set; }
        public int NbrTasksInError { get; set; }
        public int NbrOfActiveReports { get; set; }
        public int NbrOfActiveAlerts { get; set; }
        public int NbrOfActiveQueries { get; set; }
        public int NbrOfActiveDataTransfer { get; set; }
        public double SizeFilesStoredLocally { get; set; }
        public int NbrOfFilesStored { get; set; }
    }
}
