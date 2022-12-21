using Report_App_WASM.Shared;
using Report_App_WASM.Shared.SerializedParameters;
using System.Data.Common;

namespace Report_App_WASM.Server.Utils.RemoteQueryParameters
{

    public class RemoteConnectionParameter
    {
        public string? ConnnectionString { get; set; }
        public TypeDb TypeDb { get; set; }
        public string? Schema { get; set; }
        public bool UseDbSchema { get; set; }
        public int CommandTimeOut { get; set; } = 300;
        public int CommandFetchSize { get; set; } = 2000;
    }

    public class DbGenericParameters
    {
        public DbConnection? DbConnection { get; set; }
        public DbCommand? DbCommand { get; set; }
        public DbDataAdapter? DbDataAdapter { get; set; }
        public List<string> IntializationQueries = new();
    }
}
