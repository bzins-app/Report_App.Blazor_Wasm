using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_App_WASM.Shared.ApiExchanges
{
    public class TablesDescriptionsImportPayload
    {
        public int ActivityDbConnectionId { get; set; }
        public string? FilePath { get; set; }
    }
}
