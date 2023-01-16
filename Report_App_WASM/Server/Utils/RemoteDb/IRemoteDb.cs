﻿using System.Data;
using Report_App_WASM.Server.Models;
using Report_App_WASM.Shared.RemoteQueryParameters;

namespace Report_App_WASM.Server.Utils.RemoteDb;

public interface IRemoteDb : IDisposable
{
    string GetAllTablesScript(ActivityDbConnection dbInfo);
    string GetAllTablesAndColumnsScript(ActivityDbConnection dbInfo);
    string GetTableColumnInfoScript(ActivityDbConnection dbInfo, string tableName);
    Task TryConnectAsync(ActivityDbConnection dbInfo);

    Task<DataTable> RemoteDbToDatableAsync(DataTable data, RemoteDbCommandParameters run,
        ActivityDbConnection dbInfo, CancellationToken cts);
}