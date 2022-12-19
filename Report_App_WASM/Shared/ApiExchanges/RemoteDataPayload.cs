using Report_App_WASM.Shared.RemoteQueryParameters;

namespace Report_App_WASM.Shared.ApiExchanges
{
    public class RemoteDataPayload
    {
        public RemoteDbCommandParameters values { get; set; }
        // public CancellationToken Ct { get; set; }
    }
}
