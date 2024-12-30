using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using Report_App_WASM.Shared.DatabasesConnectionParameters;

namespace Report_App_WASM.Server.Utils.RemoteDb;

public class OracleRemoteDb : IRemoteDb
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public string GetAllTablesScript(DatabaseConnection dbInfo)
    {
        var script = string.Empty;
        if (CheckDbType(dbInfo))
            script = dbInfo.UseDbSchema
                ? $"SELECT 'Table' as tab_type, table_name FROM all_tables where owner='{dbInfo.DbSchema}' \r\nunion all\r\nSELECT 'View' as tab_type,  View_name FROM all_views where owner='{dbInfo.DbSchema}' \r\norder by 1,2"
                : "SELECT 'Table' as TypeValue, table_name FROM all_tables  \r\nunion all\r\nSELECT 'View' as TypeValue,  View_name FROM all_views \r\norder by 1,2";
        return script;
    }

    public string GetAllTablesAndColumnsScript(DatabaseConnection dbInfo)
    {
        var script = string.Empty;
        if (CheckDbType(dbInfo))
            script = dbInfo.UseDbSchema
                ? $"SELECT Table_name as Table_Name,column_name as Column_Name FROM ALL_TAB_COLUMNS where owner='{dbInfo.DbSchema}' order by 1,2"
                : "SELECT Table_name as Table_Name,column_name as Column_Name FROM ALL_TAB_COLUMNS order by 1,2";
        return script;
    }

    public string GetTableColumnInfoScript(DatabaseConnection dbInfo, string tableName)
    {
        var script = string.Empty;
        if (CheckDbType(dbInfo))
            script = dbInfo.UseDbSchema
                ? $"select 'Col' as tab_type,Column_name,Data_Type,Column_id from ALL_TAB_COLUMNS where table_name='{tableName}' and owner='{dbInfo.DbSchema}' order by Column_id"
                : $"select\r\n'Col' as TypeValue,\r\nColumn_name,\r\nData_Type,\r\nColumn_id\r\nfrom ALL_TAB_COLUMNS \r\nwhere table_name='{tableName}' \r\norder by Column_id";
        return script;
    }

    public async Task TryConnectAsync(DatabaseConnection dbInfo)
    {
        var param = CreateConnectionString(dbInfo);
        DbConnection conn = new OracleConnection(param.ConnnectionString);
        await conn.OpenAsync();
        await conn.DisposeAsync();
    }

    public async Task TryConnectAsync(string ConnnectionString)
    {
        DbConnection conn = new OracleConnection(ConnnectionString);
        await conn.OpenAsync();
        await conn.DisposeAsync();
    }

    public async Task<DataTable> RemoteDbToDatableAsync(DataTable data, RemoteDbCommandParameters run,
        DatabaseConnection dbInfo, CancellationToken cts)
    {
        if (CheckDbType(dbInfo))
        {
            var connectionInfo = CreateConnectionString(dbInfo);
            List<string> IntializationQueries = new();

            var DbConnection = new OracleConnection(connectionInfo.ConnnectionString);
            var DbDataAdapter = new OracleDataAdapter();
            var cmd = new OracleCommand
            {
                FetchSize = connectionInfo.CommandFetchSize,
                CommandTimeout = connectionInfo.CommandTimeOut,
                CommandType = CommandType.Text
            };
            if (run.QueryCommandParameters!.Any())
                foreach (var parameter in run.QueryCommandParameters)
                    if (parameter.ValueType is QueryCommandParameterValueType.Date
                        or QueryCommandParameterValueType.DateTime)
                    {
                        DateTime timevalue;
                        if (!string.IsNullOrEmpty(parameter.Value))
                            timevalue = DateTime.Parse(parameter.Value);
                        else if (parameter.DateOption is CalulatedDateOption.Now or CalulatedDateOption.LastRun)
                            timevalue = run.LastRunDateTime;
                        else
                            timevalue = parameter.ValueType == QueryCommandParameterValueType.Date
                                ? parameter.DateOption.GetCalculateDateTime().Date
                                : parameter.DateOption.GetCalculateDateTime();

                        OracleParameter para = new(parameter.ParameterIdentifier,
                            parameter.ValueType is QueryCommandParameterValueType.Date
                                ? OracleDbType.Date
                                : OracleDbType.TimeStamp,
                            ParameterDirection.Input)
                        {
                            Value = parameter.ValueType == QueryCommandParameterValueType.Date
                                ? (OracleDate)timevalue
                                : (OracleTimeStamp)timevalue
                        };
                        cmd.Parameters.Add(para);
                    }
                    else
                    {
                        OracleParameter para = new(parameter.ParameterIdentifier, OracleDbType.Varchar2,
                            ParameterDirection.Input)
                        {
                            Value = parameter.Value
                        };
                        if (string.IsNullOrEmpty(parameter.Value))
                        {
                            para.Value = DBNull.Value;
                            cmd.Parameters.Add(para);
                        }
                        else
                        {
                            cmd.Parameters.Add(para);
                        }
                    }

            cmd.BindByName = true;

            if (connectionInfo.UseDbSchema)
                IntializationQueries.Add(
                    "alter session set current_schema=" + connectionInfo.Schema);
            IntializationQueries.Add(
                "alter session set nls_date_format = 'DD/MM/YYYY HH24:MI:SS'  ");


            await DbConnection.OpenAsync(cts);
            cmd.Connection = DbConnection;
            foreach (var query in IntializationQueries)
            {
                cmd.CommandText = query;
                await cmd.ExecuteReaderAsync(cts);
            }

            await using var ctr = cts.Register(() => cmd.Cancel());
            DbDataAdapter.SelectCommand = cmd;
            using (DbDataAdapter)
            {
                cmd.CommandText =
                    run.QueryToRun +
                    Environment
                        .NewLine; //newline to avoid empty feedback when a comment is open at the last line without CR
                DbDataAdapter.SelectCommand = cmd;
                if (run.FillDatatableSchema) DbDataAdapter.FillSchema(data, SchemaType.Source);
                if (run.PaginatedResult)
                    DbDataAdapter.Fill(run.StartRecord, run.MaxSize, data);
                else
                    DbDataAdapter.Fill(data);

                DbDataAdapter.Dispose();
            }

            if (cts.IsCancellationRequested) data.Clear();
            await DbConnection.DisposeAsync();
        }

        return data;
    }

    private static bool CheckDbType(DatabaseConnection dbInfo)
    {
        return dbInfo.TypeDb == TypeDb.Oracle;
    }

    private RemoteConnectionParameter CreateConnectionString(DatabaseConnection dbInfo)
    {
        RemoteConnectionParameter value = new()
        {
            Schema = dbInfo.DbSchema,
            UseDbSchema = dbInfo.UseDbSchema,
            TypeDb = dbInfo.TypeDb,
            CommandFetchSize = dbInfo.CommandFetchSize,
            CommandTimeOut = dbInfo.CommandTimeOut,
            ConnnectionString =
                $"User ID={dbInfo.ConnectionLogin};Password={EncryptDecrypt.EncryptDecrypt.DecryptString(dbInfo.Password)}; Data Source={dbInfo.ConnectionPath};"
        };

        var dbparam=DatabaseConnectionParametersManager.DeserializeFromJson(dbInfo.DbConnectionParameters, dbInfo.ConnectionLogin, EncryptDecrypt.EncryptDecrypt.DecryptString(dbInfo.Password));
        value.ConnnectionString = dbparam.BuildConnectionString();

        return value;
    }
}