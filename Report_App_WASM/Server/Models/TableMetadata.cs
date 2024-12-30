﻿namespace Report_App_WASM.Server.Models;

public class TableMetadata : BaseTraceability, IExcludeAuditTrail
{
    public int Id { get; set; }
    [MaxLength(200)] public string? TableName { get; set; }
    [MaxLength(400)] public string? TableDescription { get; set; }
    [MaxLength(200)] public string? ColumnName { get; set; }
    [MaxLength(400)] public string? ColumnDescription { get; set; }
    public bool IsSnippet { get; set; }
    [MaxLength(1000)] public string MiscParamters { get; set; } = "[]";
    public virtual DatabaseConnection? DatabaseConnection { get; set; }
}