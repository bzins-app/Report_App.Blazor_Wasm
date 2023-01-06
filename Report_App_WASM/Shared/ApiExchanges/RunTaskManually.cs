using Report_App_WASM.Shared.SerializedParameters;

namespace Report_App_WASM.Shared.ApiExchanges
{
    public class RunTaskManually
    {
        public int TaskHeaderId { get; init; }
        public List<EmailRecipient>? Emails { get; init; }
        public List<QueryCommandParameter>? CustomQueryParameters { get; init; }
        public bool GenerateFiles { get; init; } = false;
    }

    public class TaskActivatePayload
    {
        public bool Activate { get; init; }
        public int TaskHeaderId { get; init; }
    }
}
