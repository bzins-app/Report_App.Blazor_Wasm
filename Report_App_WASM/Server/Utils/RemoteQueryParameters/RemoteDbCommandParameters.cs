using Report_App_WASM.Shared;
using System.Data.Common;

namespace Report_App_WASM.Server.Utils.RemoteQueryParameters
{

    public class RemoteConnectionParameter
    {
        public string? ConnnectionString { get; set; }
        public TypeDb TypeDb { get; init; }
        public string? Schema { get; init; }
        public bool UseDbSchema { get; init; }
        public int CommandTimeOut { get; init; } = 300;
        public int CommandFetchSize { get; init; } = 2000;
    }

    public class DbGenericParameters
    {
        public DbConnection? DbConnection { get; set; }
        public DbCommand? DbCommand { get; set; }
        public DbDataAdapter? DbDataAdapter { get; set; }
        public List<string> IntializationQueries = new();
    }
}
