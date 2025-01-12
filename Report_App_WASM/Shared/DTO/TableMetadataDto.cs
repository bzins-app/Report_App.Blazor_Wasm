namespace Report_App_WASM.Shared.DTO;

public class TableMetadataDto : BaseTraceabilityDto, IDto
{
    public long Id { get; set; }
    [MaxLength(600)] public string? TableName { get; set; }
    [MaxLength(600)] public string? TableDescription { get; set; }
    [MaxLength(600)] public string? ColumnName { get; set; }
    [MaxLength(600)] public string? ColumnDescription { get; set; }
    public bool IsSnippet { get; set; }
    [MaxLength(4000)] public string MiscParameters { get; set; } = "[]";
    public virtual DatabaseConnectionDto? ActivityDbConnection { get; set; }
}