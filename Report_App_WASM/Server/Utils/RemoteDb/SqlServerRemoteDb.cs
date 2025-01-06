using System.Text;
using Microsoft.Data.SqlClient;
using Report_App_WASM.Shared.DatabasesConnectionParameters;

namespace Report_App_WASM.Server.Utils.RemoteDb;

public class SqlServerRemoteDb : IRemoteDb
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public string GetAllTablesScript(ActivityDbConnection dbInfo)
    {
        var script = string.Empty;
        if (CheckDbType(dbInfo))
        {
            var dbparam=DatabaseConnectionParametersManager.DeserializeFromJson(dbInfo.DbConnectionParameters, "", "");
            script = !string.IsNullOrEmpty(dbparam.Database)
                ? $@"SELECT  case when TABLE_TYPE='BASE TABLE' then 'Table' else 'View' end as ValueType, concat(TABLE_SCHEMA,'.',TABLE_NAME) as table_name
                FROM information_schema.tables where TABLE_CATALOG='{dbparam.Database}' order by 1,2"
                : $@"SELECT  case when TABLE_TYPE='BASE TABLE' then 'Table' else 'View' end as ValueType, concat(TABLE_SCHEMA,'.',TABLE_NAME) as table_name
                FROM information_schema.tables order by 1,2";
        }
        return script;
    }

    public string GetAllTablesAndColumnsScript(ActivityDbConnection dbInfo)
    {
        var script = string.Empty;
        if (CheckDbType(dbInfo))
        {
            var dbparam=DatabaseConnectionParametersManager.DeserializeFromJson(dbInfo.DbConnectionParameters, "", "");
            if (!string.IsNullOrEmpty(dbparam.Database))
                script =
                    $"select  concat(tables.TABLE_SCHEMA,'.',tab.name) as Table_name, col.name as Column_Name  from sys.tables as tab inner join sys.columns as col on tab.object_id = col.object_id left join sys.types as t on col.user_type_id = t.user_type_id" +
                    $" inner join information_schema.tables tables on tables.TABLE_NAME=tab.name and tables.TABLE_CATALOG='{dbparam.Database}' order by 1 ,2";
            else
                script =
                    "select concat(tables.TABLE_SCHEMA,'.',tab.name) as Table_name, col.name as Column_Name  from sys.tables as tab inner join sys.columns as col on tab.object_id = col.object_id left join sys.types as t on col.user_type_id = t.user_type_id" +
                    $"  inner join information_schema.tables tables on tables.TABLE_NAME=tab.name  order by 1 ,2";
        }

        return script;
    }

    public string GetTableColumnInfoScript(ActivityDbConnection dbInfo, string tableName)
    {
        var script = string.Empty;
        if (CheckDbType(dbInfo))
            script =
                $@"select
				'Col' as Valuetype,
				COLUMN_NAME,
				DATA_TYPE,
				ORDINAL_POSITION as ColOrder
				from INFORMATION_SCHEMA.COLUMNS
				where concat(TABLE_SCHEMA,'.',TABLE_NAME) ='{tableName}'
				order by ColOrder";
        return script;
    }

    public async Task TryConnectAsync(ActivityDbConnection dbInfo)
    {
        var param = CreateConnectionString(dbInfo);
        DbConnection conn = new SqlConnection(param.ConnnectionString);
        await conn.OpenAsync();
        await conn.DisposeAsync();
    }

    public async Task TryConnectAsync(string ConnnectionString)
    {
        DbConnection conn = new SqlConnection(ConnnectionString);
        await conn.OpenAsync();
        await conn.DisposeAsync();
    }

    public async Task<DataTable> RemoteDbToDatableAsync(DataTable data, RemoteDbCommandParameters run,
        ActivityDbConnection dbInfo, CancellationToken cts)
    {
        if (CheckDbType(dbInfo))
        {
            var connectionInfo = CreateConnectionString(dbInfo);

            var DbConnection = new SqlConnection(connectionInfo.ConnnectionString);
            var DbDataAdapter = new SqlDataAdapter();
            var cmd = new SqlCommand
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

                        SqlParameter para = new(parameter.ParameterIdentifier,
                            parameter.ValueType is QueryCommandParameterValueType.Date
                                ? SqlDbType.Date
                                : SqlDbType.DateTime2)
                        {
                            Value = timevalue
                        };
                        cmd.Parameters.Add(para);
                    }
                    else
                    {
                        SqlParameter para = new(parameter.ParameterIdentifier, SqlDbType.VarChar)
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
                //{ var paginatedQueryStatement = DbDataAdapter.SelectCommand.CommandText.ToLower().RemoveSpecialCharacters().Contains("orderby") ? $" OFFSET {run.StartRecord} ROWS FETCH NEXT {run.MaxSize} ROWS ONLY " : $" order by 1  OFFSET {run.StartRecord} ROWS FETCH NEXT {run.MaxSize} ROWS ONLY ";
                //    DbDataAdapter.SelectCommand.CommandText += Environment
                //        .NewLine + paginatedQueryStatement; 
                //DbDataAdapter.Fill(data);}
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
        return dbInfo.TypeDb == TypeDb.SqlServer;
    }

    private RemoteConnectionParameter CreateConnectionString(ActivityDbConnection dbInfo)
    {
        var dbparam=DatabaseConnectionParametersManager.DeserializeFromJson(dbInfo.DbConnectionParameters, dbInfo.ConnectionLogin??"", string.IsNullOrEmpty(dbInfo.Password)?"": EncryptDecrypt.EncryptDecrypt.DecryptString(dbInfo.Password));
        RemoteConnectionParameter value = new()
        {
            TypeDb = dbInfo.TypeDb,
            CommandFetchSize = dbInfo.CommandFetchSize,
            CommandTimeOut = dbInfo.CommandTimeOut,
            ConnnectionString = dbparam.BuildConnectionString()
        };


        return value;
    }

    public async Task<bool> CkeckTableExists(ActivityDbConnection dbInfo, string query)
    {
        var remoteConnection = CreateConnectionString(dbInfo);
        await using SqlConnection conn = new(remoteConnection.ConnnectionString);
        await conn.OpenAsync();
        var sqlCommand = new SqlCommand(query, conn);
        sqlCommand.CommandTimeout = dbInfo.CommandTimeOut;
        var reader = await sqlCommand.ExecuteReaderAsync();

        if (reader.HasRows)
            while (reader.Read())
            {
                var actionType = reader.GetInt32(0);
                switch (actionType)
                {
                    case 1:
                        return true;
                    case 0:
                        return false;
                }
            }

        return false;
    }

    public async Task DeleteTable(ActivityDbConnection dbInfo, string tableName)
    {
        if (string.IsNullOrEmpty(tableName))
            throw new ArgumentNullException(nameof(tableName));
        StringBuilder query = new();
        query.Append(
            $"IF OBJECT_ID (N'{tableName}', N'U') IS NOT NULL " +
            Environment.NewLine +
            $"DROP TABLE {tableName} ;" + Environment.NewLine +
            Environment.NewLine);

        var remoteConnection = CreateConnectionString(dbInfo);
        await using SqlConnection conn = new(remoteConnection.ConnnectionString);
        await conn.OpenAsync();
        var sqlCommand = new SqlCommand(query.ToString(), conn);
        sqlCommand.CommandTimeout = dbInfo.CommandTimeOut;
        await sqlCommand.ExecuteNonQueryAsync();
    }

    public async Task CreateTable(ActivityDbConnection dbInfo, string query)
    {
        var remoteConnection = CreateConnectionString(dbInfo);
        await using SqlConnection conn = new(remoteConnection.ConnnectionString);
        await conn.OpenAsync();
        var sqlCommand = new SqlCommand(query, conn);
        sqlCommand.CommandTimeout = dbInfo.CommandTimeOut;
        await sqlCommand.ExecuteNonQueryAsync();
    }

    public async Task<MergeResult> MergeTables(ActivityDbConnection dbInfo, string query)
    {
        var remoteConnection = CreateConnectionString(dbInfo);
        SqlConnection conn = new(remoteConnection.ConnnectionString);
        await conn.OpenAsync();

        var mergeCommand = new SqlCommand(query, conn);
        mergeCommand.CommandTimeout = dbInfo.CommandTimeOut;
        var reader = await mergeCommand.ExecuteReaderAsync();
        var result = new MergeResult();

        if (reader.HasRows)
        {
            while (reader.Read())
            {
                var actionType = reader.GetString(0);
                var count = reader.GetInt32(1);

                switch (actionType)
                {
                    case "UPDATE":
                        result.UpdatedCount = count;
                        break;
                    case "INSERT":
                        result.InsertedCount = count;
                        break;
                    case "DELETE":
                        result.DeletedCount = count;
                        break;
                }
            }
        }
        else
        {
            result.UpdatedCount = 0;
            result.InsertedCount = 0;
            result.DeletedCount = 0;
        }

        await conn.DisposeAsync();
        return result;
    }

    public async Task LoadDatatableToTable(ActivityDbConnection dbInfo, DataTable data, string? targetTable)
    {
        var remoteConnection = CreateConnectionString(dbInfo);
        using var bulkCopy = new SqlBulkCopy(remoteConnection.ConnnectionString);
        bulkCopy.DestinationTableName = "dbo." + targetTable;
        bulkCopy.BulkCopyTimeout = dbInfo.CommandTimeOut;

        var totalRows = data.Rows.Count;

        if (totalRows > 1000000)
        {
            var batchsize = 1000000;
            for (var i = 0; i <= totalRows; i += batchsize)
                await bulkCopy.WriteToServerAsync(data.AsEnumerable().Distinct().Skip(i).Take(batchsize)
                    .CopyToDataTable());
        }
        else
        {
            await bulkCopy.WriteToServerAsync(data);
        }
    }
}