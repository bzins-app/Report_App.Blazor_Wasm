namespace Report_App_WASM.Shared.DTO;

public class QueryStoreDto : BaseTraceabilityDto, IDto
{
    public int Id { get; set; }
    public int IdActivity { get; set; }
    public string? ActivityName { get; set; }
    public string? Comment { get; set; }
    public string Tags { get; set; } = "[]";
    public string? QueryName { get; set; }
    public string? Query { get; set; }
    public string Parameters { get; set; } = "[]";
    public string QueryParameters { get; set; } = "[]";
    public virtual ActivityDto? Activity { get; set; }
}