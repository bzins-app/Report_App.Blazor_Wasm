namespace Report_App_WASM.Shared.ApiExchanges;

public class RunTaskManually
{
    public long TaskHeaderId { get; init; }
    public List<EmailRecipient>? Emails { get; init; }
    public List<QueryCommandParameter>? CustomQueryParameters { get; init; }
    public bool GenerateFiles { get; init; } = false;
}