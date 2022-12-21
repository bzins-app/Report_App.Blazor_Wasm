using Report_App_WASM.Shared.SerializedParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_App_WASM.Shared
{
    public class RunTaskManually
    {
        public int TaskHeaderId { get; set; }
        public List<EmailRecipient>? Emails { get; set; }
        public List<QueryCommandParameter>? CustomQueryParameters { get; set; }
        public bool GenerateFiles { get; set; } = false;
    }
}
