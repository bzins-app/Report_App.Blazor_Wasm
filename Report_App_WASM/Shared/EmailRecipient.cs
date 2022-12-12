using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_App_WASM.Shared
{
    public class EmailRecipient
    {
        public string Email { get; set; }
        public bool BCC { get; set; } = false;
    }
}
