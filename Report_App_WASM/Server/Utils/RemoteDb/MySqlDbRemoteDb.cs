using MySql.Data.MySqlClient;

namespace Report_App_WASM.Server.Utils.RemoteDb;

public class MySqlDbRemoteDb : IRemoteDb
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public string GetAllTablesScript(ActivityDbConnection dbInfo)
    {
        var script = string.Empty;
        if (CheckDbType(dbInfo))
            script = @$"SELECT  
                       TABLE_NAME
                    FROM 
                        information_schema.TABLES 
                    WHERE 
                        TABLE_SCHEMA='{dbInfo.DbSchema}'  AND  TABLE_TYPE ='BASE TABLE' order by 1;";
        return script;
    }

    public string GetAllTablesAndColumnsScript(ActivityDbConnection dbInfo)
    {
        var script = string.Empty;
        if (CheckDbType(dbInfo))
            script = @$"select 
                            tab.table_name as table_name,
                            col.column_name as column_name
                        from information_schema.tables as tab
                            inner join information_schema.columns as col
                                on col.table_schema = tab.table_schema
                                and col.table_name = tab.table_name
                        where tab.table_type = 'BASE TABLE'
                            and tab.table_schema ='{dbInfo.DbSchema}' order by 1";
        return script;
    }

    public string GetTableColumnInfoScript(ActivityDbConnection dbInfo, string tableName)
    {
        var script = string.Empty;
        if (CheckDbType(dbInfo))
            script = @$"select 
                            col.column_name as column_name
                        from information_schema.tables as tab
                            inner join information_schema.columns as col
                                on col.table_schema = tab.table_schema
                                and col.table_name = tab.table_name
                        where tab.table_type = 'BASE TABLE' and tab.table_name='{tableName}'
                            and tab.table_schema ='{dbInfo.DbSchema}'";
        return script;
    }

    public async Task TryConnectAsync(ActivityDbConnection dbInfo)
    {
        var param = CreateConnectionString(dbInfo);
        DbConnection conn = new MySqlConnection(param.ConnnectionString);
        await conn.OpenAsync();
        await conn.DisposeAsync();
    }

    public async Task<DataTable> RemoteDbToDatableAsync(DataTable data, RemoteDbCommandParameters run,
        ActivityDbConnection dbInfo, CancellationToken cts)
    {
        if (CheckDbType(dbInfo))
        {
            var connectionInfo = CreateConnectionString(dbInfo);

            var DbConnection = new MySqlConnection(connectionInfo.ConnnectionString);
            var DbDataAdapter = new MySqlDataAdapter();
            var cmd = new MySqlCommand
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

                        MySqlParameter para = new(parameter.ParameterIdentifier,
                            parameter.ValueType is QueryCommandParameterValueType.Date
                                ? MySqlDbType.Date
                                : MySqlDbType.DateTime)
                        {
                            Value = timevalue
                        };
                        cmd.Parameters.Add(para);
                    }
                    else
                    {
                        MySqlParameter para = new(parameter.ParameterIdentifier, MySqlDbType.VarChar)
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
                if (run.FillDatatableSchema) await DbDataAdapter.FillSchemaAsync(data, SchemaType.Source, cts);
                if (run.PaginatedResult)
                {
                    DbDataAdapter.SelectCommand.CommandText +=
                        Environment.NewLine + $" LIMIT {run.MaxSize} OFFSET {run.StartRecord} ";
                    await DbDataAdapter.FillAsync(data, cts);
                }
                else
                {
                    await DbDataAdapter.FillAsync(data, cts);
                }

                DbDataAdapter.Dispose();
            }

            if (cts.IsCancellationRequested) data.Clear();
            await DbConnection.DisposeAsync();
        }

        return data;
    }

    private static bool CheckDbType(ActivityDbConnection dbInfo)
    {
        return dbInfo.TypeDb == TypeDb.MySql;
    }

    private RemoteConnectionParameter CreateConnectionString(ActivityDbConnection dbInfo)
    {
        RemoteConnectionParameter value = new()
        {
            Schema = dbInfo.DbSchema,
            UseDbSchema = dbInfo.UseDbSchema,
            TypeDb = dbInfo.TypeDb,
            CommandFetchSize = dbInfo.CommandFetchSize,
            CommandTimeOut = dbInfo.CommandTimeOut
        };

        dbInfo.UseDbSchema = true;
        var databaseInfo = "";
        if (dbInfo.UseDbSchema) databaseInfo = $";database={dbInfo.DbSchema}";
        value.ConnnectionString =
            $"server={dbInfo.ConnectionPath};port={dbInfo.Port}{databaseInfo};uid={dbInfo.ConnectionLogin};Pwd={EncryptDecrypt.EncryptDecrypt.DecryptString(dbInfo.Password)};SslMode=Preferred;";

        return value;
    }
}