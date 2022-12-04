using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_App_WASM.Shared.SerializedParameters
{
    public class CronParameters
    {
        public string CronValue { get; set; }
        public DateTime? ExpireOnDateTime { get; set; } = null;
    }
}
