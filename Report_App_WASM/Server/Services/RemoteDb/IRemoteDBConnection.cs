using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using Report_App_WASM.Server.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Server.Utils.EncryptDecrypt;
using Report_App_WASM.Server.Utils.RemoteQueryParameters;
using Report_App_WASM.Shared;
using Report_App_WASM.Shared.DTO;
using Report_App_WASM.Shared.Extensions;
using Report_App_WASM.Shared.RemoteQueryParameters;
using Report_App_WASM.Shared.SerializedParameters;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Text;

namespace Report_App_BlazorServ.Services.RemoteDb
{
    public interface IRemoteDbConnection
    {
        Task<SubmitResult> TestConnectionAsync(ActivityDbConnectionDTO parameter);
        Task<DataTable> RemoteDbToDatableAsync(RemoteDbCommandParameters run, CancellationToken cts, int taskId = 0);
        Task<bool> CkeckTableExists(string query);
        Task CreateTable(string query);
        Task LoadDatatableToTable(DataTable data, string targetTable);
        Task<MergeResult> MergeTables(string query);
        Task DeleteTable(string tableName);
    }
    public class RemoteDbConnection : IRemoteDbConnection, IDisposable
    {
        readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public RemoteDbConnection(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private bool _first = true;
        private DateTime _fillTimeStamp;

        private async Task<int> GetDataTransferActivity()
        {
            return await _context.Activity.Where(a => a.ActivityType == ActivityType.TargetDB.ToString()).Select(a => a.ActivityId).FirstOrDefaultAsync();
        }

        public async Task<bool> CkeckTableExists(string query)
        {
            var activityId = await GetDataTransferActivity();
            var remoteConnection = GetConnectionString(activityId);
            await using SqlConnection conn = new(remoteConnection.ConnnectionString);
            await conn.OpenAsync();
            var sqlCommand = new SqlCommand(query, conn);
            var reader = await sqlCommand.ExecuteReaderAsync();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var actionType = reader.GetInt32(0);
                    await conn.DisposeAsync();
                    switch (actionType)
                    {
                        case 1:
                            return true;
                        case 0:
                            return false;
                    }
                }
            }

            return false;
        }

        public async Task DeleteTable(string tableName)
        {
            StringBuilder query = new();
            query.Append(
                 "IF OBJECT_ID (N'" + tableName + "', N'U') IS NOT NULL " +
                Environment.NewLine +
                "DROP TABLE " + tableName + ";" + Environment.NewLine +
                Environment.NewLine);

            var activityId = await GetDataTransferActivity();
            var remoteConnection = GetConnectionString(activityId);
            await using SqlConnection conn = new(remoteConnection.ConnnectionString);
            await conn.OpenAsync();
            var sqlCommand = new SqlCommand(query.ToString(), conn);
            await sqlCommand.ExecuteNonQueryAsync();
            await conn.DisposeAsync();
        }

        public async Task CreateTable(string query)
        {
            var activityId = await GetDataTransferActivity();
            var remoteConnection = GetConnectionString(activityId);
            await using SqlConnection conn = new(remoteConnection.ConnnectionString);
            await conn.OpenAsync();
            var sqlCommand = new SqlCommand(query, conn);
            await sqlCommand.ExecuteNonQueryAsync();
            await conn.DisposeAsync();
        }

        public async Task<MergeResult> MergeTables(string query)
        {
            var activityId = await GetDataTransferActivity();
            var remoteConnection = GetConnectionString(activityId);
            SqlConnection conn = new(remoteConnection.ConnnectionString);
            await conn.OpenAsync();

            var mergeCommand = new SqlCommand(query, conn);
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
            await conn.DisposeAsync();
            return result;
        }

        public async Task LoadDatatableToTable(DataTable data, string targetTable)
        {
            var activityId = await GetDataTransferActivity();
            var remoteConnection = GetConnectionString(activityId);
            await using SqlConnection conn = new(remoteConnection.ConnnectionString);
            await conn.OpenAsync();
            using (var bulkCopy = new SqlBulkCopy(conn))
            {
                bulkCopy.DestinationTableName = "dbo." + targetTable;

                int totalRows = data.Rows.Count;

                if (totalRows > 1000000)
                {
                    int batchsize = 1000000;
                    for (int i = 0; i <= totalRows; i += batchsize)
                    {
                        await bulkCopy.WriteToServerAsync(data.AsEnumerable().Skip(i).Take(batchsize).CopyToDataTable());
                    }
                }
                else
                {
                    await bulkCopy.WriteToServerAsync(data);
                }

            }
            await conn.DisposeAsync();
        }

        private RemoteConnectionParameter CreateConnectionString(ActivityDbConnectionDTO parameter)
        {
            RemoteConnectionParameter value = new() { Schema = parameter.DbSchema, UseDbSchema = parameter.UseDbSchema, TypeDb = parameter.TypeDb, CommandFetchSize = parameter.CommandFetchSize, CommandTimeOut = parameter.CommandTimeOut };
            if (parameter.TypeDb == TypeDb.Oracle)
            {
                value.ConnnectionString = $"User ID={parameter.ConnectionLogin};Password={parameter.Password}; Data Source={parameter.ConnectionPath};";
            }
            else if (parameter.TypeDb == TypeDb.SQLServer)
            {
                string windowsAuthentication = ";Integrated Security=SSPI";
                string connectionString;
                string databaseInfo = "";
                if (parameter.UseDbSchema)
                {
                    databaseInfo = $";Database={parameter.DbSchema}";
                }

                if (parameter.ADAuthentication)
                {
                    connectionString = $"server ={parameter.ConnectionPath}{databaseInfo}{windowsAuthentication};";
                }
                else
                {
                    connectionString = $"server ={parameter.ConnectionPath}{databaseInfo};User Id={parameter.ConnectionLogin};Password={parameter.Password};";
                }

                if (parameter.IntentReadOnly)
                {
                    connectionString += "applicationintent=readonly;";
                }
                connectionString += "Encrypt=False;";
                value.ConnnectionString = connectionString;
            }
            else if (parameter.TypeDb == TypeDb.DB2)
            {
                parameter.UseDbSchema = true;
                string databaseInfo = "";
                if (parameter.UseDbSchema)
                {
                    databaseInfo = $";Initial Catalog={parameter.DbSchema}";
                }
                value.ConnnectionString = $"Provider=DB2OLEDB.1;Data Source={parameter.ConnectionPath}{databaseInfo};User ID={parameter.ConnectionLogin};Password={parameter.Password};";
            }
            else
            {
                parameter.UseDbSchema = true;
                string databaseInfo = "";
                if (parameter.UseDbSchema)
                {
                    databaseInfo = $";database={parameter.DbSchema}";
                }
                value.ConnnectionString = $"server={parameter.ConnectionPath};port={parameter.Port}{databaseInfo};uid={parameter.ConnectionLogin};password={parameter.Password};";
            }

            return value;
        }

        private RemoteConnectionParameter GetConnectionString(int activityId)
        {
            ActivityDbConnectionDTO conValue = new();
            conValue = _context.ActivityDbConnection.AsNoTracking().Include(a => a.Activity).Where(a => a.Activity.ActivityId == activityId).ProjectTo<ActivityDbConnectionDTO>(_mapper.ConfigurationProvider).SingleOrDefault();
            conValue.Password = EncryptDecrypt.DecryptString(conValue.Password);

            return CreateConnectionString(conValue);
        }

        private async Task TryConnectAsync(TypeDb TypeDb, string connectionString)
        {
            DbConnection conn;

            if (TypeDb == TypeDb.Oracle)
            {
                conn = new OracleConnection(connectionString);
            }
            else if (TypeDb == TypeDb.SQLServer)
            {
                conn = new SqlConnection(connectionString);
            }
            else if (TypeDb == TypeDb.DB2)
            {
                conn = new OleDbConnection(connectionString);
            }
            else
            {
                conn = new MySqlConnection(connectionString);
            }
            await conn.OpenAsync();
            await conn.DisposeAsync();
        }

        private async Task<DataTable> DbToDataTableAsync(DbGenericParameters dbConnector, RemoteDbCommandParameters run, DataTable dataTable, CancellationToken cts)
        {
            await dbConnector.DbConnection.OpenAsync(cts);
            dbConnector.DbCommand.Connection = dbConnector.DbConnection;
            foreach (var query in dbConnector.IntializationQueries)
            {
                dbConnector.DbCommand.CommandText = query;
                await dbConnector.DbCommand.ExecuteReaderAsync(cts);
            }

            using CancellationTokenRegistration ctr = cts.Register(() => dbConnector.DbCommand.Cancel());
            dbConnector.DbDataAdapter.SelectCommand = dbConnector.DbCommand;
            using (dbConnector.DbDataAdapter)
            {
                dbConnector.DbCommand.CommandText = run.QueryToRun + Environment.NewLine;//newline to avoid empty feedback when a comment is open at the last line without CR
                dbConnector.DbDataAdapter.SelectCommand = dbConnector.DbCommand;
                if (run.FillDatatableSchema)
                {
                    dbConnector.DbDataAdapter.FillSchema(dataTable, SchemaType.Source);
                }
                if (run.PaginatedResult)
                {
                    dbConnector.DbDataAdapter.Fill(run.StartRecord, run.MaxSize, dataTable);
                }
                else
                {
                    dbConnector.DbDataAdapter.Fill(dataTable);
                }

                dbConnector.DbDataAdapter.Dispose();
            }
            if (cts.IsCancellationRequested)
            {
                dataTable.Clear();
            }
            await dbConnector.DbConnection.DisposeAsync();
            return dataTable;
        }
        public async Task<SubmitResult> TestConnectionAsync(ActivityDbConnectionDTO parameter)
        {
            try
            {
                var conParam = CreateConnectionString(parameter);
                await TryConnectAsync(conParam.TypeDb, conParam.ConnnectionString);
                return new SubmitResult { Success = true, Message = "OK" };
            }
            catch (Exception e)
            {
                return new SubmitResult { Success = false, Message = e.Message };
            }

        }

        void OnInitialized(object sender, DataRowChangeEventArgs args)
        {
            if (_first)
            {
                _fillTimeStamp = DateTime.Now;
                _first = false;
            }
        }

        public async Task<DataTable> RemoteDbToDatableAsync(RemoteDbCommandParameters run, CancellationToken cts, int taskId = 0)
        {
            DataTable values;
            var attempts = 0;
            if (run.Test)
            {
                attempts = 3;
            }
            do
            {
                string activityName = await _context.Activity.Where(a => a.ActivityId == run.ActivityId).Select(a => a.ActivityName).FirstOrDefaultAsync();
                // ApplicationLogTask logTask = new() { ActivityId = run.ActivityId, ActivityName = activityName, StartDateTime = DateTime.Now, JobDescription = run.QueryInfo, Type = "Attempt" };
                var logTask = new ApplicationLogTaskDetails { TaskId = taskId, Step = "Fetch data", Info = run.QueryInfo };
                try
                {
                    attempts++;
                    var connectionInfo = GetConnectionString(run.ActivityId);
                    DataTable dataTable = new();
                    dataTable.RowChanged += OnInitialized;
                    DateTime start = DateTime.Now;
                    _first = true;
                    _fillTimeStamp = DateTime.Now;
                    DbGenericParameters dbConnector = new();

                    if (connectionInfo.TypeDb == TypeDb.Oracle)
                    {
                        dbConnector.DbConnection = new OracleConnection(connectionInfo.ConnnectionString);
                        dbConnector.DbDataAdapter = new OracleDataAdapter();
                        var cmd = new OracleCommand { FetchSize = connectionInfo.CommandFetchSize, CommandTimeout = connectionInfo.CommandTimeOut, CommandType = CommandType.Text };
                        if (run.QueryCommandParameters.Any())
                        {
                            foreach (var parameter in run.QueryCommandParameters)
                            {
                                if (parameter.ValueType is QueryCommandParameterValueType.Date
                                    or QueryCommandParameterValueType.DateTime)
                                {
                                    DateTime timevalue;
                                    if (!string.IsNullOrEmpty(parameter.value))
                                    {
                                        timevalue = DateTime.Parse(parameter.value);
                                    }
                                    else if (parameter.DateOption is CalulatedDateOption.Now or CalulatedDateOption.LastRun)
                                    {
                                        timevalue = run.LastRunDateTime;
                                    }
                                    else
                                    {
                                        timevalue = parameter.ValueType == QueryCommandParameterValueType.Date ? parameter.DateOption.GetCalculateDateTime().Date : parameter.DateOption.GetCalculateDateTime();
                                    }

                                    OracleParameter para = new(parameter.ParameterIdentifier, parameter.ValueType is QueryCommandParameterValueType.Date ? OracleDbType.Date : OracleDbType.TimeStamp,
                                        ParameterDirection.Input)
                                    {
                                        Value = parameter.ValueType == QueryCommandParameterValueType.Date ? (OracleDate)timevalue : (OracleTimeStamp)timevalue
                                    };
                                    cmd.Parameters.Add(para);
                                }
                                else
                                {
                                    OracleParameter para = new(parameter.ParameterIdentifier, OracleDbType.Varchar2,
                                        ParameterDirection.Input)
                                    {
                                        Value = parameter.value
                                    };
                                    if (string.IsNullOrEmpty(parameter.value))
                                    {
                                        para.Value = DBNull.Value;
                                        cmd.Parameters.Add(para);
                                    }
                                    else
                                    {
                                        cmd.Parameters.Add(para);
                                    }
                                }
                            }
                        }

                        cmd.BindByName = true;
                        dbConnector.DbCommand = cmd;

                        if (connectionInfo.UseDbSchema)
                        {
                            dbConnector.IntializationQueries.Add("alter session set current_schema=" + connectionInfo.Schema);
                        }
                        dbConnector.IntializationQueries.Add("alter session set nls_date_format = 'DD/MM/YYYY HH24:MI:SS'  ");
                    }
                    else if (connectionInfo.TypeDb == TypeDb.SQLServer)
                    {
                        dbConnector.DbConnection = new SqlConnection(connectionInfo.ConnnectionString);
                        dbConnector.DbDataAdapter = new SqlDataAdapter();
                        var cmd = new SqlCommand { CommandTimeout = connectionInfo.CommandTimeOut, CommandType = CommandType.Text };
                        if (run.QueryCommandParameters.Any())
                        {
                            foreach (var parameter in run.QueryCommandParameters)
                            {
                                if (parameter.ValueType is QueryCommandParameterValueType.Date
                                    or QueryCommandParameterValueType.DateTime)
                                {
                                    DateTime timevalue;
                                    if (!string.IsNullOrEmpty(parameter.value))
                                    {
                                        timevalue = DateTime.Parse(parameter.value);
                                    }
                                    else if (parameter.DateOption is CalulatedDateOption.Now or CalulatedDateOption.LastRun)
                                    {
                                        timevalue = run.LastRunDateTime;
                                    }
                                    else
                                    {
                                        timevalue = parameter.ValueType == QueryCommandParameterValueType.Date ? parameter.DateOption.GetCalculateDateTime().Date : parameter.DateOption.GetCalculateDateTime();
                                    }

                                    SqlParameter para = new(parameter.ParameterIdentifier, parameter.ValueType is QueryCommandParameterValueType.Date ? SqlDbType.Date : SqlDbType.DateTime2)
                                    {
                                        Value = timevalue
                                    };
                                    cmd.Parameters.Add(para);
                                }
                                else
                                {
                                    SqlParameter para = new(parameter.ParameterIdentifier, SqlDbType.VarChar)
                                    {
                                        Value = parameter.value
                                    };
                                    if (string.IsNullOrEmpty(parameter.value))
                                    {
                                        para.Value = DBNull.Value;
                                        cmd.Parameters.Add(para);
                                    }
                                    else
                                    {
                                        cmd.Parameters.Add(para);
                                    }
                                }
                            }
                        }

                        dbConnector.DbCommand = cmd;
                    }
                    else if (connectionInfo.TypeDb == TypeDb.DB2)
                    {
                        dbConnector.DbConnection = new OleDbConnection(connectionInfo.ConnnectionString);
                        dbConnector.DbDataAdapter = new OleDbDataAdapter();
                        dbConnector.DbCommand = new OleDbCommand { CommandTimeout = connectionInfo.CommandTimeOut, CommandType = CommandType.Text };
                    }
                    else
                    {
                        dbConnector.DbConnection = new MySqlConnection(connectionInfo.ConnnectionString);
                        dbConnector.DbDataAdapter = new MySqlDataAdapter();
                        var cmd = new MySqlCommand { CommandTimeout = connectionInfo.CommandTimeOut, CommandType = CommandType.Text };
                        if (run.QueryCommandParameters.Any())
                        {
                            foreach (var parameter in run.QueryCommandParameters)
                            {
                                if (parameter.ValueType is QueryCommandParameterValueType.Date
                                    or QueryCommandParameterValueType.DateTime)
                                {
                                    DateTime timevalue;
                                    if (!string.IsNullOrEmpty(parameter.value))
                                    {
                                        timevalue = DateTime.Parse(parameter.value);
                                    }
                                    else if (parameter.DateOption is CalulatedDateOption.Now or CalulatedDateOption.LastRun)
                                    {
                                        timevalue = run.LastRunDateTime;
                                    }
                                    else
                                    {
                                        timevalue = parameter.ValueType == QueryCommandParameterValueType.Date ? parameter.DateOption.GetCalculateDateTime().Date : parameter.DateOption.GetCalculateDateTime();
                                    }

                                    MySqlParameter para = new(parameter.ParameterIdentifier, parameter.ValueType is QueryCommandParameterValueType.Date ? MySqlDbType.Date : MySqlDbType.DateTime)
                                    {
                                        Value = timevalue
                                    };
                                    cmd.Parameters.Add(para);
                                }
                                else
                                {
                                    MySqlParameter para = new(parameter.ParameterIdentifier, MySqlDbType.VarChar)
                                    {
                                        Value = parameter.value
                                    };
                                    if (string.IsNullOrEmpty(parameter.value))
                                    {
                                        para.Value = DBNull.Value;
                                        cmd.Parameters.Add(para);
                                    }
                                    else
                                    {
                                        cmd.Parameters.Add(para);
                                    }
                                }
                            }
                        }

                        dbConnector.DbCommand = cmd;
                    }
                    values = await DbToDataTableAsync(dbConnector, run, dataTable, cts);
                    dataTable.RowChanged -= OnInitialized;

                    DateTime fill;
                    if (values.Rows.Count == 0)
                    {
                        fill = DateTime.Now;
                    }
                    else
                    {
                        fill = _fillTimeStamp;
                    }
                    DateTime end = DateTime.Now;
                    if (!run.Test)
                    {
                        ApplicationLogQueryExecution logQuery = new()
                        {
                            ActivityId = run.ActivityId,
                            Database = connectionInfo.Schema,
                            TypeDb = connectionInfo.TypeDb.ToString(),
                            CommandTimeOut = connectionInfo.CommandTimeOut,
                            StartDateTime = start,
                            TotalDuration = end - start,
                            SQLExcecutionDuration = fill - start,
                            DownloadDuration = end - fill,
                            EndDateTime = end,
                            TransferBeginDateTime = fill,
                            QueryName = run.QueryInfo,
                            Query = run.QueryToRun,
                            NbrOfRows = values.Rows.Count,
                            ActivityName = activityName
                        };
                        await _context.AddAsync(logQuery);
                    }
                    return values;
                    //break; // Sucess! Lets exit the loop!
                }
                catch (Exception ex)
                {
                    if (attempts == 4)
                        throw;

                    if (cts.IsCancellationRequested)
                        throw;

                    var delay = attempts switch
                    {
                        1 => 10000,
                        2 => 60 * 1000,
                        3 => 10 * 60 * 1000,
                        _ => 10000,
                    };
                    if (!run.Test)
                    {
                        logTask.Info += $" Exception caught on attempt {attempts} - will retry after delay in {delay / 1000} seconds " + ex.Message;
                        logTask.Step += $": attempt {attempts}";
                        await _context.AddAsync(logTask);
                        await Task.Delay(delay, cts).WaitAsync(cts);
                    }
                }
            } while (true);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
