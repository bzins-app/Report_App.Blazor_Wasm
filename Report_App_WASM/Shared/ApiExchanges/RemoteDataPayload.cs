using Report_App_WASM.Shared.RemoteQueryParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_App_WASM.Shared.ApiExchanges
{
    public class RemoteDataPayload
    {
        public RemoteDbCommandParameters? values { get; set; }
        public CancellationToken Ct { get; set; }
    }
}
