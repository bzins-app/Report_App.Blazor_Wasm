using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_App_WASM.Shared.SerializedParameters
{
    public class DataTransferParameters
    {
        public string DataTransferTargetTableName { get; set; }
        public bool DataTransferCreateTable { get; set; } = false;
        public bool DataTransferUsePK { get; set; } = false;
        public string DataTransferCommandBehaviour { get; set; }
        public List<string> DataTransferPK { get; set; } = new List<string>();
    }
}
