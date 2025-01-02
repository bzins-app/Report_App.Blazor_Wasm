using System.Data.OleDb;
using Report_App_WASM.Shared.DatabasesConnectionParameters;

#pragma warning disable CA1416

namespace Report_App_WASM.Server.Utils.RemoteDb;

public class OlebDbDbRemoteDb : IRemoteDb
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public string GetAllTablesScript(ActivityDbConnection dbInfo)
    {
        var script = string.Empty;
        return script;
    }

    public string GetAllTablesAndColumnsScript(ActivityDbConnection dbInfo)
    {
        var script = string.Empty;
        return script;
    }

    public string GetTableColumnInfoScript(ActivityDbConnection dbInfo, string tableName)
    {
        var script = string.Empty;
        return script;
    }

    public async Task TryConnectAsync(ActivityDbConnection dbInfo)
    {
        var param = CreateConnectionString(dbInfo);
        DbConnection conn = new OleDbConnection(param.ConnnectionString);
        await conn.OpenAsync();
        await conn.DisposeAsync();
    }

    public async Task TryConnectAsync(string ConnnectionString)
    {
        DbConnection conn = new OleDbConnection(ConnnectionString);
        await conn.OpenAsync();
        await conn.DisposeAsync();
    }


    public async Task<DataTable> RemoteDbToDatableAsync(DataTable data, RemoteDbCommandParameters run,
        ActivityDbConnection dbInfo, CancellationToken cts)
    {
        if (CheckDbType(dbInfo))
        {
            var connectionInfo = CreateConnectionString(dbInfo);

            var DbConnection = new OleDbConnection(connectionInfo.ConnnectionString);
            var DbDataAdapter = new OleDbDataAdapter();
            var cmd = new OleDbCommand
                { CommandTimeout = connectionInfo.CommandTimeOut, CommandType = CommandType.Text };

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

    private static bool CheckDbType(ActivityDbConnection dbInfo)
    {
        return dbInfo.TypeDb == TypeDb.Db2;
    }

    private RemoteConnectionParameter CreateConnectionString(ActivityDbConnection dbInfo)
    {
       /* RemoteConnectionParameter value = new()
        {
            Schema = dbInfo.DbSchema,
            UseDbSchema = dbInfo.UseDbSchema,
            TypeDb = dbInfo.TypeDb,
            CommandFetchSize = dbInfo.CommandFetchSize,
            CommandTimeOut = dbInfo.CommandTimeOut
        };

        dbInfo.UseDbSchema = true;
        var databaseInfo = "";
        if (dbInfo.UseDbSchema) databaseInfo = $";Initial Catalog={dbInfo.DbSchema}";
        value.ConnnectionString =
            $"Provider=DB2OLEDB.1;Data Source={dbInfo.ConnectionPath}{databaseInfo};User ID={dbInfo.ConnectionLogin};Password={EncryptDecrypt.EncryptDecrypt.DecryptString(dbInfo.Password)};";*/

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