using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_App_WASM.Shared
{
    public class DepositPathTest
    {
        public bool UseSFTPProtocol { get; set; }
        public string? FilePath { get; set; }
        public bool TryToCreateFolder { get; set; }
        public int SFTPConfigurationId { get; set; }
    }
}
