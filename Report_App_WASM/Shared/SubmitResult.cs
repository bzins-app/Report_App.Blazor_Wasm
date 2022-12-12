using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_App_WASM.Shared
{
    public class SubmitResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Value { get; set; }
    }
}
