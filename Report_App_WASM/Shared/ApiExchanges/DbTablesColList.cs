using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_App_WASM.Shared.ApiExchanges
{
    public class DbTablesColList
    {
        public Dictionary<string, string>? Values { get; set; } = new();
        public bool HasDescription { get; set; }
    }
}
