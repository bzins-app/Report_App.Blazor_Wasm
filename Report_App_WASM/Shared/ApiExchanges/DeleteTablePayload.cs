namespace Report_App_WASM.Shared.ApiExchanges;

public class DeleteTablePayload
{
    public string TableName { get; set; } = string.Empty;
    public long IdDataTransfer { get; set; }
}