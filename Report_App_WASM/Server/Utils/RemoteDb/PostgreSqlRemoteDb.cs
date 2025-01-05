using Npgsql;
using NpgsqlTypes;
using Report_App_WASM.Shared.DatabasesConnectionParameters;

namespace Report_App_WASM.Server.Utils.RemoteDb;

public class PostgreSqlRemoteDb : IRemoteDb
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public string GetAllTablesScript(DatabaseConnection dbInfo)
    {
        var script = string.Empty;
        if (CheckDbType(dbInfo))
        {var dbparam=DatabaseConnectionParametersManager.DeserializeFromJson(dbInfo.DbConnectionParameters, "", "");
            script = !string.IsNullOrEmpty(dbparam.Database)
                ? $@"				SELECT  case when TABLE_TYPE='BASE TABLE' then 'Table' else 'View' end as ValueType, TABLE_NAME as table_name
				FROM information_schema.tables where TABLE_CATALOG='{dbparam.Database}' and TABLE_SCHEMA='public' order by 1,2"
                : @"				SELECT  case when TABLE_TYPE='BASE TABLE' then 'Table' else 'View' end as ValueType, TABLE_NAME as table_name
				FROM information_schema.tables where TABLE_SCHEMA='public' order by 1,2";
        }

        return script;
    }

    public string GetAllTablesAndColumnsScript(DatabaseConnection dbInfo)
    {
        var script = string.Empty;
        if (CheckDbType(dbInfo))
        {var dbparam=DatabaseConnectionParametersManager.DeserializeFromJson(dbInfo.DbConnectionParameters, "", "");
            script = @$"select t.table_name,
                    c.column_name 
                    from information_schema.tables t
                    inner join information_schema.columns c on c.table_name = t.table_name 
                                                    and c.table_schema = t.table_schema
                    where t.table_catalog='{dbparam.Database}' and t.table_schema='public' 
                          and t.table_type = 'BASE TABLE'
                    order by 1,2;";
        }

        return script;
    }

    public string GetTableColumnInfoScript(DatabaseConnection dbInfo, string tableName)
    {
        var script = string.Empty;
        if (CheckDbType(dbInfo))
        {var dbparam=DatabaseConnectionParametersManager.DeserializeFromJson(dbInfo.DbConnectionParameters, "", "");
            script = @$"                select
				'Col' as Valuetype,
				c.COLUMN_NAME,
				c.DATA_TYPE,
				c.ORDINAL_POSITION as ColOrder
				from INFORMATION_SCHEMA.COLUMNS c
                join information_schema.tables t  on t.TABLE_NAME=c.TABLE_NAME
				where c.TABLE_NAME ='{tableName}'
                and t.TABLE_CATALOG='{dbparam.Database}' 
				order by ColOrder";
        }

        return script;
    }

    public async Task TryConnectAsync(DatabaseConnection dbInfo)
    {
        var param = CreateConnectionString(dbInfo);
        DbConnection conn = new NpgsqlConnection(param.ConnnectionString);
        await conn.OpenAsync();
        await conn.DisposeAsync();
    }

    public async Task TryConnectAsync(string ConnnectionString)
    {
        DbConnection conn = new NpgsqlConnection(ConnnectionString);
        await conn.OpenAsync();
        await conn.DisposeAsync();
    }

    public async Task<DataTable> RemoteDbToDatableAsync(DataTable data, RemoteDbCommandParameters run,
        DatabaseConnection dbInfo, CancellationToken cts)
    {
        if (CheckDbType(dbInfo))
        {
            var connectionInfo = CreateConnectionString(dbInfo);

            var DbConnection = new NpgsqlConnection(connectionInfo.ConnnectionString);
            var DbDataAdapter = new NpgsqlDataAdapter();
            var cmd = new NpgsqlCommand
                { CommandTimeout = connectionInfo.CommandTimeOut, CommandType = CommandType.Text };
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

                        NpgsqlParameter para = new(parameter.ParameterIdentifier,
                            NpgsqlDbType.Date)
                        {
                            Value = timevalue
                        };
                        cmd.Parameters.Add(para);
                    }
                    else
                    {
                        NpgsqlParameter para = new(parameter.ParameterIdentifier, NpgsqlDbType.Varchar)
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


            await DbConnection.OpenAsync(cts);
            cmd.Connection = DbConnection;

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
        return dbInfo.TypeDb == TypeDb.PostgreSql;
    }

    private RemoteConnectionParameter CreateConnectionString(DatabaseConnection dbInfo)
    {
        var dbparam=DatabaseConnectionParametersManager.DeserializeFromJson(dbInfo.DbConnectionParameters, dbInfo.ConnectionLogin, EncryptDecrypt.EncryptDecrypt.DecryptString(dbInfo.Password));
        RemoteConnectionParameter value = new()
        {
            TypeDb = dbInfo.TypeDb,
            CommandFetchSize = dbInfo.CommandFetchSize,
            CommandTimeOut = dbInfo.CommandTimeOut,
            ConnnectionString = dbparam.BuildConnectionString()
        };

        return value;
    }
}