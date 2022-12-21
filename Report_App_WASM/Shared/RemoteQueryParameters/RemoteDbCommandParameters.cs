using Report_App_WASM.Shared;
using Report_App_WASM.Shared.SerializedParameters;
using System.Data.Common;

namespace Report_App_WASM.Shared.RemoteQueryParameters
{
    public class RemoteDbCommandParameters
    {
        public int ActivityId { get; set; }
        public string? FileName { get; set; }
        public string? QueryToRun { get; set; }
        public List<QueryCommandParameter> QueryCommandParameters { get; set; } = new();
        public string? QueryInfo { get; set; }
        public bool FillDatatableSchema { get; set; } = false;
        public bool Test { get; set; } = false;
        public bool PaginatedResult { get; set; } = false;
        public int StartRecord { get; set; } = 0;
        public int MaxSize { get; set; } = 1000000;
        public DateTime LastRunDateTime { get; set; } = DateTime.Now;
    }

}
