﻿using MySql.Data.MySqlClient;
using Report_App_WASM.Shared.DatabasesConnectionParameters;

namespace Report_App_WASM.Server.Utils.RemoteDb;

public class MariaDbRemoteDb : IRemoteDb
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public string GetAllTablesScript(ActivityDbConnection dbInfo)
    {
        var script = string.Empty;
        if (CheckDbType(dbInfo))
        {var dbparam=DatabaseConnectionParametersManager.DeserializeFromJson(dbInfo.DbConnectionParameters, "", "");
            script = @$"		
        SELECT  case when TABLE_TYPE='BASE TABLE' then 'Table' else 'View' end as ValueType, TABLE_NAME as table_name
		FROM information_schema.tables t  
                where TABLE_SCHEMA='{dbparam.Database}' 
                order by 1,2";
        }
        return script;
    }

    public string GetAllTablesAndColumnsScript(ActivityDbConnection dbInfo)
    {
        var script = string.Empty;
        if (CheckDbType(dbInfo))
        {var dbparam=DatabaseConnectionParametersManager.DeserializeFromJson(dbInfo.DbConnectionParameters, "", "");
            script = @$"select 
                            tab.table_name as table_name,
                            col.column_name as column_name
                        from information_schema.tables as tab
                            inner join information_schema.columns as col
                                on col.table_schema = tab.table_schema
                                and col.table_name = tab.table_name
                        where tab.table_type = 'BASE TABLE'
                            and tab.table_schema ='{dbparam.Database}' order by 1";
        }

        return script;
    }

    public string GetTableColumnInfoScript(ActivityDbConnection dbInfo, string tableName)
    {
        var script = string.Empty;
        if (CheckDbType(dbInfo))
        {var dbparam=DatabaseConnectionParametersManager.DeserializeFromJson(dbInfo.DbConnectionParameters, "", "");
            script = @$"select
				'Col' as Valuetype,
				c.COLUMN_NAME,
				c.DATA_TYPE,
				c.ORDINAL_POSITION as ColOrder
				from INFORMATION_SCHEMA.COLUMNS c
                join information_schema.tables t  on t.TABLE_NAME=c.TABLE_NAME
				where c.TABLE_NAME ='{tableName}'
                and t.TABLE_SCHEMA='{dbparam.Database}' 
				order by ColOrder";
        }

        return script;
    }

    public async Task TryConnectAsync(ActivityDbConnection dbInfo)
    {
        var param = CreateConnectionString(dbInfo);
        DbConnection conn = new MySqlConnection(param.ConnnectionString);
        await conn.OpenAsync();
        await conn.DisposeAsync();
    }

    public async Task TryConnectAsync(string ConnnectionString)
    {
        DbConnection conn = new MySqlConnection(ConnnectionString);
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
                    //DbDataAdapter.SelectCommand.CommandText +=
                    //    Environment.NewLine + $" LIMIT {run.MaxSize} OFFSET {run.StartRecord} ";
                    await DbDataAdapter.FillAsync(run.StartRecord, run.MaxSize, cts, data);
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
        return dbInfo.TypeDb == TypeDb.MariaDb;
    }

    private RemoteConnectionParameter CreateConnectionString(ActivityDbConnection dbInfo)
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