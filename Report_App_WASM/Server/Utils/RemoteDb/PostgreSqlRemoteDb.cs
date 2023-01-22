using System.Data;
using System.Data.Common;
using Npgsql;
using NpgsqlTypes;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Utils.RemoteQueryParameters;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.Extensions;
using Report_App_WASM.Shared.RemoteQueryParameters;
using Report_App_WASM.Shared.SerializedParameters;

namespace Report_App_WASM.Server.Utils.RemoteDb;

public class PostgreSqlRemoteDb : IDisposable, IRemoteDb
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public string GetAllTablesScript(ActivityDbConnection dbInfo)
    {
        var script = string.Empty;
        if (CheckDbType(dbInfo))
            script = dbInfo.UseDbSchema
                ? $"SELECT table_name FROM information_schema.tables where table_schema='{dbInfo.DbSchema}' order by 1"
                : "SELECT table_name FROM information_schema.tables order by 1";
        return script;
    }

    public string GetAllTablesAndColumnsScript(ActivityDbConnection dbInfo)
    {
        var script = string.Empty;
        if (CheckDbType(dbInfo))
            script = @$"select t.table_name,
                    c.column_name 
                    from information_schema.tables t
                    inner join information_schema.columns c on c.table_name = t.table_name 
                                                    and c.table_schema = t.table_schema
                    where table_schema='{dbInfo.DbSchema}'
                          and t.table_type = 'BASE TABLE'
                    order by 1,2;";
        return script;
    }

    public string GetTableColumnInfoScript(ActivityDbConnection dbInfo, string tableName)
    {
        var script = string.Empty;
        if (CheckDbType(dbInfo))
            script = @$"select t.table_name,
                    c.column_name 
                    from information_schema.tables t
                    inner join information_schema.columns c on c.table_name = t.table_name 
                                                    and c.table_schema = t.table_schema
                    where table_schema='{dbInfo.DbSchema}' and c.column_name='{tableName}'
                          and t.table_type = 'BASE TABLE'
                    order by 1";
        return script;
    }

    public async Task TryConnectAsync(ActivityDbConnection dbInfo)
    {
        var param = CreateConnectionString(dbInfo);
        DbConnection conn = new NpgsqlConnection(param.ConnnectionString);
        await conn.OpenAsync();
        await conn.DisposeAsync();
    }

    public async Task<DataTable> RemoteDbToDatableAsync(DataTable data, RemoteDbCommandParameters run,
        ActivityDbConnection dbInfo, CancellationToken cts)
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
                if (run.FillDatatableSchema)  DbDataAdapter.FillSchema(data, SchemaType.Source);
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

    private static bool CheckDbType(ActivityDbConnection dbInfo)
    {
        return dbInfo.TypeDb == TypeDb.PostgreSql;
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
        if (dbInfo.UseDbSchema) databaseInfo = $";Database={dbInfo.DbSchema}";
        value.ConnnectionString =
            $"Server={dbInfo.ConnectionPath};Port={dbInfo.Port}{databaseInfo};User Id={dbInfo.ConnectionLogin};Password={EncryptDecrypt.EncryptDecrypt.DecryptString(dbInfo.Password)};";

        return value;
    }
}