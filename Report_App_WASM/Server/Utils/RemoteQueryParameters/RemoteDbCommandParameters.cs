using Report_App_WASM.Shared.SerializedParameters;
using System.Data.Common;

namespace Report_App_WASM.Server.Utils.RemoteQueryParameters
{
    public class RemoteDbCommandParameters
    {
        public int ActivityId { get; set; }
        public string FileName { get; set; }
        public string QueryToRun { get; set; }
        public List<QueryCommandParameter> QueryCommandParameters { get; set; } = new();
        public string QueryInfo { get; set; }
        public bool FillDatatableSchema { get; set; } = false;
        public bool Test { get; set; } = false;
        public bool PaginatedResult { get; set; } = false;
        public int StartRecord { get; set; } = 0;
        public int MaxSize { get; set; } = 1000000;
        public DateTime LastRunDateTime { get; set; } = DateTime.Now;
    }

    public class RemoteConnectionParameter
    {
        public string ConnnectionString { get; set; }
        public string DbType { get; set; }
        public string Schema { get; set; }
        public bool UseDbSchema { get; set; }
        public int CommandTimeOut { get; set; } = 300;
        public int CommandFetchSize { get; set; } = 2000;
    }

    public class DbGenericParameters
    {
        public DbConnection DbConnection { get; set; }
        public DbCommand DbCommand { get; set; }
        public DbDataAdapter DbDataAdapter { get; set; }
        public List<string> IntializationQueries = new();
    }
}
