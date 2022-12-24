using Report_App_WASM.Shared.SerializedParameters;

namespace Report_App_WASM.Shared.ApiExchanges
{
    public class RunTaskManually
    {
        public int TaskHeaderId { get; set; }
        public List<EmailRecipient>? Emails { get; set; }
        public List<QueryCommandParameter>? CustomQueryParameters { get; set; }
        public bool GenerateFiles { get; set; } = false;
    }
}
