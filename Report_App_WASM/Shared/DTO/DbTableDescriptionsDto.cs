namespace Report_App_WASM.Shared.DTO;

public class DbTableDescriptionsDto : BaseTraceabilityDto, IDto
{
    public int Id { get; set; }
    public string? TableName { get; set; }
    public string? TableDescription { get; set; }
    public string? ColumnName { get; set; }
    public string? ColumnDescription { get; set; }
    public bool IsSnippet { get; set; }
    public virtual ActivityDbConnectionDto? ActivityDbConnection { get; set; }
}