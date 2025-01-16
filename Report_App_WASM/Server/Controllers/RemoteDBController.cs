﻿using Report_App_WASM.Server.Services.RemoteDb;
using Report_App_WASM.Server.Utils.FIles;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Report_App_WASM.Server.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
[Route("api/[controller]/[Action]")]
[ApiController]
public class RemoteDbController : ControllerBase, IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RemoteDbController> _logger;
    private readonly IRemoteDatabaseActionsHandler _remoteDb;

    public RemoteDbController(IRemoteDatabaseActionsHandler remoteDb, ILogger<RemoteDbController> logger,
        ApplicationDbContext context)
    {
        _remoteDb = remoteDb;
        _logger = logger;
        _context = context;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    [HttpPost]
    public async Task<IActionResult> TestConnection(ApiCrudPayload<DatabaseConnection> value)
    {
        try
        {
            value.EntityValue!.Password = EncryptDecrypt.DecryptString(value.EntityValue.Password!);
            var result = await _remoteDb.TestConnectionAsync(value.EntityValue);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Ok(new SubmitResult { Success = false, Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteDataTransferTable(ApiCrudPayload<DeleteTablePayload> value)
    {
        try
        {
            await _remoteDb.DeleteTable(value.EntityValue.TableName, value.EntityValue.IdDataTransfer);
            return Ok(new SubmitResult { Success = true });
        }
        catch (Exception ex)
        {
            return Ok(new SubmitResult { Success = false, Message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> RemoteDbGetValues(RemoteDataPayload payload, CancellationToken ct)
    {
        var log = new AdHocQueryExecutionLog
        {
            QueryId = payload.QueryId,
            ProviderName = payload.ProviderName,
            DataProviderId = payload.Values.DataProviderId,
            JobDescription = payload.QueryName,
            RunBy = User?.Identity?.Name,
            Type = "Grid",
            Error = false,
            Result = "Ok"
        };

        var total = 0;
        if (payload.CalculateTotalElements)
        {
            var originalQuery = payload.Values.QueryToRun;
            if (payload.Values.QueryToRun != null)
            {
                var query = await GetQueryTotal(payload.Values.DataProviderId, payload.Values.QueryToRun);
                payload.Values.QueryToRun = query;
            }

            try
            {
                var dataTotal = await _remoteDb.RemoteDbToDatableAsync(payload.Values!, ct);
                payload.Values.QueryToRun = originalQuery;
                if (dataTotal != null)
                    total = Convert.ToInt32(dataTotal.Rows[0][0]);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error get total: {payload.Values.FileName} {ex.Message} ",
                    payload.Values.FileName);
                total = payload.Values.MaxSize + 1;
            }
        }

        if (!string.IsNullOrEmpty(payload.SortingDirection))
        {
            var query = await GetQuerySorted(payload.Values.DataProviderId, payload.Values.QueryToRun, payload.ColumSorting, payload.SortingDirection);
            payload.Values.QueryToRun = query;
        }

        try
        {
            var data = await _remoteDb.RemoteDbToDatableAsync(payload.Values!, ct);
            if (payload.PivotTable)
            {
                var selectColumns = data.Columns.Cast<DataColumn>().Take(10);
                var nbrCols = data.Columns.Count;
                var maxCols = payload.PivotTableNbrColumns;
                if (nbrCols > maxCols)
                    for (var x = maxCols; x < nbrCols; x++)
                        if (data.Columns.Count > maxCols)
                            data.Columns.RemoveAt(maxCols);

                log.Type = "Pivot table";
                log.NbrOfRows = data.Rows.Count;
                log.EndDateTime = DateTime.Now;
                log.DurationInSeconds = (log.EndDateTime - log.StartDateTime).Seconds;
                if (payload.LogPayload)
                {
                    await _context.AddAsync(log);
                    await _context.SaveChangesAsync();
                }

                var result = new SubmitResultRemoteData { Success = true, Value = data.ToDictionnary() };
                return Ok(result);
            }
            else
            {
                log.EndDateTime = DateTime.Now;
                log.DurationInSeconds = (log.EndDateTime - log.StartDateTime).Seconds;
                log.NbrOfRows = data.Rows.Count;
                if (payload.LogPayload)
                {
                    await _context.AddAsync(log);
                    await _context.SaveChangesAsync();
                }

                var result = new SubmitResultRemoteData
                    { Success = true, Value = data.ToDictionnary(), TotalElements = total };
                return Ok(result);
            }
        }
        catch (Exception e)
        {
            log.Error = true;
            log.Result = ct.IsCancellationRequested ? "Cancelled" : e.Message.Take(440).ToString();
            log.EndDateTime = DateTime.Now;
            log.DurationInSeconds = (log.EndDateTime - log.StartDateTime).Seconds;
            if (payload.LogPayload)
            {
                await _context.AddAsync(log);
                await _context.SaveChangesAsync();
            }

            var result = new SubmitResultRemoteData
            {
                Success = false,
                Message = e.Message,
                Value = new List<Dictionary<string, object>>()
            };
            return Ok(result);
        }
    }

    private async Task<TypeDb> GetDbType(long dataProviderId)
    {
        return await _context.DatabaseConnection
            .Where(a => a.DataProvider.DataProviderId == dataProviderId)
            .Select(a => a.TypeDb)
            .FirstOrDefaultAsync();
    }

    private async Task<string> GetQueryTotal(long dataProviderId, string query)
    {
        var _typeDb = await GetDbType(dataProviderId);

        if (_typeDb == TypeDb.SqlServer && query.ToLower().RemoveSpecialCharacters().Contains("orderby"))
            query += Environment.NewLine + " OFFSET 0 Rows";

        return $@"select  count(*) from ( 
                    {query} 
                    ) a";
    }


    private async Task<string> GetQuerySorted(long dataProviderId, string query, string sortingCol, string sortingDirection)
    {
        var _typeDb = await GetDbType(dataProviderId);

        if (_typeDb == TypeDb.SqlServer && query.ToLower().RemoveSpecialCharacters().Contains("orderby"))
            query += Environment.NewLine + " OFFSET 0 Rows";

        return $@"select  * from ( 
                    {query} 
                    ) a  order by {sortingCol} {sortingDirection}";
    }

    [HttpPost]
    public async Task<FileResult?> RemoteDbExtractValuesAsync(RemoteDataPayload payload, CancellationToken ct)
    {
        var log = new AdHocQueryExecutionLog
        {
            QueryId = payload.QueryId,
            ProviderName = payload.ProviderName,
            DataProviderId = payload.Values.DataProviderId,
            JobDescription = payload.QueryName,
            RunBy = User?.Identity?.Name,
            Type = "Grid extraction",
            Error = false,
            Result = "Ok"
        };

        try
        {
            _logger.LogInformation("Grid extraction: Start " + payload.Values.FileName, payload.Values.FileName);
            var queriesMaxSizeExtract = await _context.DatabaseConnection
                .Where(a => a.DataProvider.DataProviderId == payload.Values.DataProviderId)
                .Select(a => a.AdHocQueriesMaxNbrofRowsFetched)
                .FirstOrDefaultAsync(ct);
            payload.Values.MaxSize = queriesMaxSizeExtract + 1;
            var items = await _remoteDb.RemoteDbToDatableAsync(payload.Values, ct);
            var fileName = $"{payload.Values.FileName} {DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            log.NbrOfRows = items.Rows.Count;
            var file = CreateFile.ExcelFromDatable(fileName,
                new ExcelCreationDatatable(payload.Values.FileName, new ExcelTemplate(),
                    items.AsEnumerable().Take(queriesMaxSizeExtract).CopyToDataTable()));
            _logger.LogInformation($"Grid extraction: End {fileName} {items.Rows.Count} lines",
                $"{fileName} {items.Rows.Count} lines");
            log.EndDateTime = DateTime.Now;
            log.DurationInSeconds = (log.EndDateTime - log.StartDateTime).Seconds;
            await _context.AddAsync(log);
            await _context.SaveChangesAsync();
            return File(file.Content, file.ContentType, file.FileName);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            log.Error = true;
            log.Result = e.Message.Take(440).ToString();
            log.EndDateTime = DateTime.Now;
            log.DurationInSeconds = (log.EndDateTime - log.StartDateTime).Seconds;
            await _context.AddAsync(log);
            await _context.SaveChangesAsync();
            return null!;
        }
    }

    [HttpPost]
    public async Task<FileResult?> ExtractDbDescriptionsAsync([FromBody] int DbconnectionID, CancellationToken ct)
    {
        try
        {
            var fileDesc = "Db Description";
            _logger.LogInformation("Db Description extraction: Start ");
            var values = _context.TableMetadata.Where(a => a.DatabaseConnection.DatabaseConnectionId == DbconnectionID);
            var fileName = $"{fileDesc} {DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            var file = CreateFile.ExcelFromCollection(fileName, fileDesc, await values.ToListAsync(ct));
            _logger.LogInformation($"Grid extraction: End {fileName}", $"{fileName}");
            return File(file.Content, file.ContentType, file.FileName);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null!;
        }
    }

    [HttpGet]
    public async Task<DbTablesColList> GetTablesListAsync(int dataProviderId, CancellationToken ct)
    {
        var listTables = new DbTablesColList();
        try
        {
            var script = await _remoteDb.GetAllTablesScript(dataProviderId);
            if (!string.IsNullOrEmpty(script))
            {
                var parameters = new RemoteDbCommandParameters
                    { DataProviderId = dataProviderId, QueryToRun = script, Test = true };
                var data = await _remoteDb.RemoteDbToDatableAsync(parameters, ct);
                var description = await _context.DatabaseConnection
                    .Where(a => a.DataProvider.DataProviderId == dataProviderId)
                    .AsNoTracking()
                    .Select(a => new
                    {
                        tableDesc = a.UseTableMetaData,
                        a.UseTableMetaDataFromAnotherProvider,
                        ConnectId = a.UseTableMetaDataFromAnotherProvider ? a.IdTableMetaData : a.DatabaseConnectionId
                    })
                    .FirstOrDefaultAsync(ct);

                var tables = data.AsEnumerable().Select(selector: r => new TablesColsInfo
                    { TypeValue = r.Field<string>(0), Name = r.Field<string>(1) }).ToList();
                if (tables != null)
                {
                    if (description.tableDesc || description.UseTableMetaDataFromAnotherProvider)
                    {
                        var prework = await _context.TableMetadata
                            .Where(a => a.DatabaseConnection.DatabaseConnectionId == description.ConnectId)
                            .AsNoTracking()
                            .Select(a => new { a.TableName, a.TableDescription })
                            .Distinct()
                            .ToListAsync(ct);
                        listTables.Values = (from a in tables
                            join b in prework on a.Name equals b.TableName into c
                            from d in c.DefaultIfEmpty()
                            select new TablesColsInfo
                                { Name = a.Name, Description = d?.TableDescription, TypeValue = a.TypeValue }).ToList();
                        listTables.HasDescription = prework.Any();
                    }
                    else
                    {
                        listTables.Values = tables;
                    }
                }
            }

            return listTables;
        }
        catch (Exception)
        {
            return listTables;
        }
    }

    [HttpGet]
    public async Task<DbTablesColList> GetColumnListAsync(int dataProviderId, string table, CancellationToken ct)
    {
        var listCols = new DbTablesColList();
        try
        {
            var script = await _remoteDb.GetTableColumnInfoScript(dataProviderId, table);
            Console.WriteLine(script);
            if (!string.IsNullOrEmpty(script))
            {
                var parameters = new RemoteDbCommandParameters
                    { DataProviderId = dataProviderId, QueryToRun = script, Test = true };
                var data = await _remoteDb.RemoteDbToDatableAsync(parameters, ct);
                var description = await _context.DatabaseConnection
                    .Where(a => a.DataProvider.DataProviderId == dataProviderId)
                    .AsNoTracking()
                    .Select(a => new
                    {
                        tableDesc = a.UseTableMetaData,
                        a.UseTableMetaDataFromAnotherProvider,
                        ConnectId = a.UseTableMetaDataFromAnotherProvider ? a.IdTableMetaData : a.DatabaseConnectionId
                    })
                    .FirstOrDefaultAsync(ct);

                if (data.Rows.Count > 0)
                {
                    var cols = data.AsEnumerable().Select(selector: r => new TablesColsInfo
                            { TypeValue = r.Field<string>(0), Name = r.Field<string>(1), ColType = r.Field<string>(2) })
                        .ToList();
                    if (cols != null)
                    {
                        if ((description.tableDesc || description.UseTableMetaDataFromAnotherProvider) && await _context
                                .TableMetadata
                                .Where(a => a.DatabaseConnection.DatabaseConnectionId == description.ConnectId &&
                                            a.TableName == table)
                                .AnyAsync(ct))
                        {
                            var desc = await _context.TableMetadata
                                .Where(a => a.DatabaseConnection.DatabaseConnectionId == description.ConnectId &&
                                            a.TableName == table)
                                .AsNoTracking()
                                .Select(a => new { a.ColumnName, a.ColumnDescription, a.IsSnippet })
                                .ToListAsync();
                            if (desc != null)
                            {
                                listCols.Values.AddRange(desc.Where(a => a.IsSnippet).Select(a => new TablesColsInfo
                                {
                                    Name = a.ColumnName!,
                                    Description = a.ColumnDescription ?? string.Empty,
                                    IsSnippet = true
                                }).ToList());
                                listCols.HasDescription = true;
                                listCols.Values.AddRange(cols.Distinct().Select(col => new TablesColsInfo
                                {
                                    Name = col.Name,
                                    TypeValue = col.TypeValue,
                                    ColType = col.ColType,
                                    ColOrder = col.ColOrder,
                                    Description =
                                        desc.FirstOrDefault(a => a.ColumnName == col.Name && !a.IsSnippet)
                                            ?.ColumnDescription ?? string.Empty
                                }));
                            }
                            else
                            {
                                listCols.Values.AddRange(cols);
                            }
                        }
                        else
                        {
                            listCols.Values.AddRange(cols);
                        }
                    }
                }
            }

            return listCols;
        }
        catch (Exception)
        {
            return listCols;
        }
    }
}