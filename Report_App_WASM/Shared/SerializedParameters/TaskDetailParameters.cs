using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Report_App_WASM.Shared.SerializedParameters
{
    public class TaskDetailParameters
    {
        public string? FileName { get; set; }
        public string EncodingType { get; set; } = "UTF8";
        public ExcelTemplate ExcelTemplate { get; set; } = new ExcelTemplate();
        public bool SeparateExcelFile { get; set; } = false;
        public bool RemoveHeader { get; set; } = false;
        public bool GenerateIfEmpty { get; set; } = false;
        public bool AddValidationSheet { get; set; } = false;
        public string? ExcelTabName { get; set; }
        public string? DataTransferTargetTableName { get; set; }
        public bool DataTransferCreateTable { get; set; } = false;
        public bool DataTransferUsePK { get; set; } = false;
        public string? DataTransferCommandBehaviour { get; set; }
        public List<string> DataTransferPK { get; set; } = new List<string>();
    }
}
